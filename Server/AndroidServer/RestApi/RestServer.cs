using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RestApi
{
    public class RestServer : IDisposable
    {
        public string Address { get; }

        private readonly HttpServer server;
        private readonly List<RestController> controllers = new List<RestController>();
        private readonly List<RestAction> restActions = new List<RestAction>();

        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private bool shouldShutdown = false;
        private bool isInsideServerRequest = false;

        public List<string> AllowedOrigins { get; } = new() { "*" };

        public RestServer(string address)
        {
            Address = address;
            server = new HttpServer(address);
            server.OnRequest += OnServerRequest;
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            if (isInsideServerRequest)
                shouldShutdown = true;
            else
                server.Stop();
        }

        public void AddController(RestController controller)
        {
            var type = controller.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                RestAttribute rest = method.GetCustomAttribute<RestAttribute>();
                if (rest == null)
                    continue;

                var restAction = new RestAction
                {
                    Method = method,
                    Route = controller.Route + "/" + rest.Route,
                    Verb = rest.Verb,
                    Controller = controller,
                    Parameters = method.GetParameters()
                };

                restActions.Add(restAction);
            }

            controller.Server = this;
            controllers.Add(controller);
        }

        private async void OnServerRequest(object sender, HttpListenerContext e)
        {
            var request = e.Request;
            var response = e.Response;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            var domain = request.UrlReferrer?.ToString();

            if (string.IsNullOrWhiteSpace(domain) || !AllowedOrigins.Any(a => MatchingRoute(a, domain)))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = "Origin not allowed";
                response.Close();
                isInsideServerRequest = false;
                return;
            }

            if (domain.EndsWith("/"))
                domain = domain[..^1];

            response.AppendHeader("Access-Control-Allow-Origin", domain);
            response.AppendHeader("Access-Control-Allow-Headers", "*");
            response.AppendHeader("Access-Control-Allow-Methods", "*");

            isInsideServerRequest = true;

            var verb = HttpVerb.Get;
            switch (request.HttpMethod)
            {
                case "GET":
                    verb = HttpVerb.Get;
                    break;
                case "POST":
                    verb = HttpVerb.Post;
                    break;
                case "PUT":
                    verb = HttpVerb.Put;
                    break;
                case "DELETE":
                    verb = HttpVerb.Delete;
                    break;
                case "OPTIONS":
                    verb = HttpVerb.Options;
                    break;
            }

            var url = request.RawUrl;
            if (url == null)
            {
                isInsideServerRequest = false;
                return;
            }

            int queryStartIndex = url.LastIndexOf('?');
            if (queryStartIndex != -1)
                url = url.Substring(0, queryStartIndex);

            var urlParams = request.QueryString;

            response.StatusCode = (int)HttpStatusCode.OK;

            foreach (var act in restActions)
            {
                if (MatchingRoute(act.Route, url) && (verb == act.Verb || verb == HttpVerb.Options))
                {
                    if (verb == HttpVerb.Options)
                    {
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.AddHeader("Allow", act.Verb.ToString().ToUpper());
                        response.Close();
                        isInsideServerRequest = false;
                        return;
                    }

                    act.Controller.CurrentContext = e;
                    act.Controller.CurrentBody = GetBodyString(request);
                    var paramsToPass = act.Parameters.Length == 0 ? null : new object[act.Parameters.Length];

                    if (act.Parameters.Length != 0)
                    {
                        var parameters = act.Parameters;

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            ParameterInfo item = parameters[i];
                            var query = urlParams.Get(item.Name);
                            if (query == null)
                            {
                                badRequest($"Missing parameter: \"{item.Name}\" expected but not given");
                                isInsideServerRequest = false;
                                return;
                            }

                            var type = item.ParameterType;

                            if (type == typeof(float))
                            {
                                if (float.TryParse(query, out var floatVal))
                                    paramsToPass[i] = floatVal;
                                else
                                {
                                    badRequest($"Parameter type mismatch: \"{query}\" is not a float");
                                    isInsideServerRequest = false;
                                    return;
                                }
                            }
                            else if (type == typeof(int))
                            {
                                if (int.TryParse(query, out var intVal))
                                    paramsToPass[i] = intVal;
                                else
                                {
                                    badRequest($"Parameter type mismatch: \"{query}\" is not an integer");
                                    isInsideServerRequest = false;
                                    return;
                                }
                            }
                            else if (type == typeof(ulong))
                            {
                                if (ulong.TryParse(query, out ulong intVal))
                                    paramsToPass[i] = intVal;
                                else
                                {
                                    badRequest($"Parameter type mismatch: \"{query}\" is not a uint64");
                                    isInsideServerRequest = false;
                                    return;
                                }
                            }
                            else if (type == typeof(bool))
                            {
                                if (bool.TryParse(query, out var boolVal))
                                    paramsToPass[i] = boolVal;
                                else
                                {
                                    badRequest($"Parameter type mismatch: \"{query}\" is not a boolean");
                                    isInsideServerRequest = false;
                                    return;
                                }
                            }
                            else/* if (type == typeof(string))*/
                            {
                                paramsToPass[i] = query;
                            }
                        }
                    }

                    try
                    {
                        var result = act.Method.Invoke(act.Controller, paramsToPass);

                        if (!server.IsOpen)
                            return;

                        if (result == null)
                        {
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.Close();
                            isInsideServerRequest = false;
                            if (shouldShutdown)
                                Stop();
                            return;
                        }

                        var returnType = act.Method.ReturnType;

                        if (result is Task)
                        {
                            if (returnType.IsGenericType)
                            {
                                var asyncResult = await (dynamic)result;
                                result = asyncResult;
                            }
                            else
                            {
                                await (dynamic)result;
                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.Close();
                                isInsideServerRequest = false;
                                if (shouldShutdown)
                                    Stop();
                                return;
                            }
                        }

                        switch (result)
                        {
                            case RestResponse restResponse:
                                {
                                    var json = JsonConvert.SerializeObject(restResponse.ReponseBody, serializerSettings);
                                    response.StatusCode = (int)restResponse.StatusCode;
                                    response.StatusDescription = restResponse.StatusDescription ?? restResponse.StatusCode.ToString();
                                    response.OutputStream.Write(FromUTF8(json));
                                    response.OutputStream.Close();
                                }
                                isInsideServerRequest = false;
                                return;
                            case string stringResponse:
                                {
                                    response.OutputStream.Write(FromUTF8(stringResponse));
                                    response.OutputStream.Close();
                                }
                                isInsideServerRequest = false;
                                return;
                            default:
                                {
                                    var json = JsonConvert.SerializeObject(result, serializerSettings);
                                    response.OutputStream.Write(FromUTF8(json));
                                    response.OutputStream.Close();
                                }
                                isInsideServerRequest = false;
                                if (shouldShutdown)
                                    Stop();
                                return;
                        }
                    }
                    catch (Exception ex)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        response.StatusDescription = ex.Message;
                        response.Close();
                        Console.WriteLine(ex.Message);
                        isInsideServerRequest = false;
                        if (shouldShutdown)
                            Stop();
                        return;
                    }
                }
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusDescription = "The requested route could not be found";
            response.Close();

            isInsideServerRequest = false;

            void badRequest(string message)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = message;
                response.Close();
            }

            if (shouldShutdown)
                Stop();
        }

        private static string GetBodyString(HttpListenerRequest request)
        {
            var bytes = new byte[request.ContentLength64];
            int readCount = request.InputStream.Read(bytes, 0, bytes.Length);
            //TODO assert valid 
            return ToUTF8(bytes);
        }

        private static byte[] FromUTF8(string v)
        {
            return Encoding.UTF8.GetBytes(v);
        }

        private static string ToUTF8(byte[] v)
        {
            return Encoding.UTF8.GetString(v);
        }

        private static bool MatchingRoute(string a, string b)
        {
            const char separator = '/';

            //TODO dit kan sneller
            var aSeg = a.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var bSeg = b.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (aSeg.Length != bSeg.Length)
                return false;

            for (int i = 0; i < aSeg.Length; i++)
                if (aSeg[i] != bSeg[i])
                    return false;

            return true;
        }

        private struct RestAction
        {
            public MethodInfo Method;
            public HttpVerb Verb;
            public string Route;
            public RestController Controller;
            public ParameterInfo[] Parameters;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
