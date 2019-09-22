using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Tiny
{
    class Web
    {

        private readonly HttpListener listener = new HttpListener();
        private Semaphore sem;

        public async Task RunAsync(int listenCount, int port)
        {
            string prefix = string.Format("http://*:{0}/", port);

            listener.IgnoreWriteExceptions = true;

            // Add the server bindings:
            Console.WriteLine("TinyWeb Listening on {0} ...", prefix);
            listener.Prefixes.Add(prefix);

            listener.Start();

            sem = new Semaphore(listenCount, listenCount);
            int requestId = 0;

            // Open and keep open listenCount listeners
            while (true)
            {
                sem.WaitOne();
                requestId++;

                await StartListener(requestId);
            }
        }

        private async Task StartListener(int requestId)
        {
            // Wait for a request context
            HttpListenerContext context = await listener.GetContextAsync();

            // Allow a new listener to be set up
            sem.Release();

            // Log request to console
            //Console.WriteLine("{0}: {1}", requestId, context.Request.RawUrl);

            // process the current request
            // replace this with your own request handler
            await UnderConstruction(context);
        }


        // Echo the raw URL as a plain text response
        private async Task UnderConstruction(HttpListenerContext context)
        {
            await Task.Run(() =>
            {
                const string message = "Under construction ...";

                try
                {
                    // Handle request here - every URL returns message as plain text
                    context.Response.ContentType = "text/plain";

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
