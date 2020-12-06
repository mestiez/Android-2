using System;
using System.Net;
using System.Threading;

namespace RestApi
{
    public class HttpServer : IDisposable
    {
        public string Address { get; }
        public event EventHandler<HttpListenerContext> OnRequest;

        private readonly HttpListener listener;
        private readonly Thread listenerThread;

        public bool IsOpen => listener.IsListening;

        public HttpServer(string address)
        {
            Address = address;
            listener = new HttpListener();
            listener.Prefixes.Add(address);

            listenerThread = new Thread(Listen);
        }

        public void Start()
        {
            if (!listener.IsListening)
            {
                try
                {
                    listener.Start();
                }
                catch (HttpListenerException e)
                {
                    throw new Exception("Port is probably already in use...\n" + e.Message);
                }
                catch (Exception)
                {
                    throw;
                }

                listenerThread.Start();
            }
        }

        public void Stop()
        {
            if (listener.IsListening)
                listener.Stop();
        }

        private void Listen()
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                OnRequest?.Invoke(this, context);
            }
        }

        public void Dispose()
        {
            listener.Close();
        }
    }
}
