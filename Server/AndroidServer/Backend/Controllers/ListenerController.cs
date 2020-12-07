using AndroidServer.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Backend.Controllers
{
    public class ListenerController : RestController
    {
        private AndroidService Android => AndroidService.Instance;

        public override string Route => "listener";

        [Rest(HttpVerb.Get, "types")]
        public IEnumerable<ListenerType> GetListenerTypes()
        {
            return ListenerTypes.GetListenerTypes();
        }

        [Rest(HttpVerb.Get, "ui")]
        public ListenerUIInfo GetListenerUI(string typeID)
        {
            var type = ListenerTypes.GetRawType(typeID);
            if (type == null)
                return null;

            bool hideBaseMembers = type.GetCustomAttribute<HideListenerBaseAttribute>() != null;

            var ui = new ListenerUIInfo();

            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute<UiVariableTypeAttribute>(true);
                if (attribute == null)
                    continue;

                var entry = new ListenerUIEntry
                {
                    Name = property.Name,
                    Type = attribute.VariableType
                };

                if (property.DeclaringType == type)
                    ui.Variables.Add(entry);
                else if (!hideBaseMembers)
                {
                    ui.Variables.Insert(0, entry);
                }
            }

            return ui;
        }

        [Rest(HttpVerb.Post, "setting")]
        public RestResponse SetListenerSetting(string id, string propertyName)
        {
            var value = DeserialiseBody<JObject>();

            var listener = Android.AndroidInstances.SelectMany(a => a.Value.Listeners).FirstOrDefault(a => a.ID == id);
            if (listener == null)
                return RestResponse.NotFound;

            var type = listener.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                return RestResponse.NotFound;

            var attr = property.GetCustomAttribute<UiVariableTypeAttribute>();

            try
            {
                var obj = value;
                var token = obj.GetValue("data");

                switch (attr.VariableType)
                {
                    case VariableType.String:
                    case VariableType.TextChannel:
                    case VariableType.VoiceChannel:
                    case VariableType.TextArea:
                    case VariableType.RoleID:
                    case VariableType.EmoteID:
                        {
                            var s = token.ToObject<string>();
                            property.SetValue(listener, s);
                            listener.OnPropertyChange(propertyName, s);
                        }
                        break;
                    case VariableType.Number:
                        {
                            float f = token.ToObject<float>();
                            property.SetValue(listener, f);
                            listener.OnPropertyChange(propertyName, f);
                        }
                        break;
                    case VariableType.Boolean:
                        {
                            bool b = token.ToObject<bool>();
                            property.SetValue(listener, b);
                            listener.OnPropertyChange(propertyName, b);
                        }
                        break;
                    default:
                        {
                            property.SetValue(listener, value);
                            listener.OnPropertyChange(propertyName, value);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                return RestResponse.BadRequest;
            }

            return RestResponse.Ok;
        }

        [Rest(HttpVerb.Get, "setting")]
        public object GetListenerSetting(string id, string propertyName)
        {
            var listener = Android.FindListener(id);
            if (listener == null)
                return null;

            var type = listener.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                return null;

            var result = property.GetValue(listener);

            return new
            {
                data = result
            };
        }

        [Rest(HttpVerb.Get, "")]
        public ListenerInfo GetListener(string id)
        {
            var found = Android.FindListener(id);
            if (found == null)
                return null;

            return ListenerInfo.Create(found);
        }

        [Rest(HttpVerb.Post, "active")]
        public void SetListenerActive(string id, bool active)
        {
            var found = Android.FindListener(id);
            if (found == null)
                return;

            found.Active = active;
        }

        [Rest(HttpVerb.Get, "filter")]
        public ResponseFilters GetListenerFilter(string id)
        {
            var found = Android.FindListener(id);
            if (found == null)
                return null;

            return found.ResponseFilters;
        }

        [Rest(HttpVerb.Post, "filter")]
        public void SetListenerFilter(string id)
        {
            var filters = DeserialiseBody<ResponseFilters>();

            var found = Android.FindListener(id);
            if (found == null)
                return;

            found.ResponseFilters = filters ?? new ResponseFilters();

            found.ResponseFilters.Roles = found.ResponseFilters.Roles.Where(r => r != null).ToList();
            found.ResponseFilters.Users = found.ResponseFilters.Users.Where(r => r != null).ToList();
        }
    }
}
