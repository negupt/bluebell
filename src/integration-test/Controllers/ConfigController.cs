using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HeliumIntegrationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : Controller
    {
        // GET api/values
        [HttpGet]
        public Config Get()
        {
            return App.config;
        }

        // TODO - remove this - for ease of testing
        [HttpGet("{key}/{value}")]
        public Config SetValue(string key, string value)
        {
            int val;
            key = key.ToLower();

            switch (key)
            {
                case "threads":
                    if (int.TryParse(value, out val))
                    {
                        Console.WriteLine("Threads: {0}", val);

                        if (val == 0)
                        {
                            foreach (var ct in App.tokens)
                            {
                                ct.Cancel();
                            }

                            App.tokens.Clear();
                            App.tasks.Clear();
                        }

                        else if (val > App.config.Threads)
                        {
                            Task t;
                            CancellationTokenSource ct;

                            for (int i = App.config.Threads; i < val; i++)
                            {
                                ct = new CancellationTokenSource();
                                t = App.smoker.RunLoop(i, App.config, ct.Token);

                                App.tokens.Add(ct);
                                App.tasks.Add(t);
                            }
                        }

                        else if (val < App.config.Threads)
                        {
                            int ndx;

                            while (val < App.tokens.Count)
                            {
                                ndx = App.tokens.Count - 1;

                                App.tokens[ndx].Cancel();
                                App.tokens.RemoveAt(ndx);
                                App.tasks.RemoveAt(ndx);
                            }
                        }

                        App.config.Threads = val;
                    }

                    return App.config;

                case "sleep":
                    if (int.TryParse(value, out val))
                    {
                        if (val >= 0)
                        {
                            Console.WriteLine("Sleep: {0}", val);
                            App.config.SleepMs = val;
                        }
                    }

                    return App.config;

                default:
                    return App.config;
            }
        }

        [HttpPut("{key}")]
        public Config SetValue(string key)
        {
            int val;
            key = key.ToLower();

            string body;
            byte[] b = new byte[100];

            int len = Request.Body.Read(b, 0, b.Length);

            body = System.Text.ASCIIEncoding.Default.GetString(b, 0, len);

            switch (key)
            {
                case "threads":
                    if (int.TryParse(body, out val))
                    {
                        Console.WriteLine("Threads: {0}", val);

                        if (val == 0)
                        {
                            foreach(var ct in App.tokens)
                            {
                                ct.Cancel();
                            }

                            App.tokens.Clear();
                            App.tasks.Clear();
                        }

                        else if (val > App.config.Threads)
                        {
                            Task t;
                            CancellationTokenSource ct;

                            for (int i = App.config.Threads; i < val; i++)
                            {
                                ct = new CancellationTokenSource();
                                t = App.smoker.RunLoop(i, App.config, ct.Token);

                                App.tokens.Add(ct);
                                App.tasks.Add(t);
                            }
                        }

                        else if (val < App.config.Threads)
                        {
                            int ndx;

                            while (val < App.tokens.Count)
                            {
                                ndx = App.tokens.Count - 1;

                                App.tokens[ndx].Cancel();
                                App.tokens.RemoveAt(ndx);
                                App.tasks.RemoveAt(ndx);
                            }
                        }

                        App.config.Threads = val;
                    }

                    return App.config;

                case "sleep":
                    if (int.TryParse(body, out val))
                    {
                        if (val >= 0)
                        {
                            Console.WriteLine("Sleep: {0}", val);
                            App.config.SleepMs = val;
                        }
                    }

                    return App.config;

                case "host":
                    return App.config;

                default:
                    return App.config;
            }
        }
    }
}
