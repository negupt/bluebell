using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    public class App
    {
        static readonly string defaultInputFile = "integration-test.json";
        public static readonly Config config = new Config();
        public static readonly List<Task> tasks = new List<Task>();
        public static readonly List<CancellationTokenSource> tokens = new List<CancellationTokenSource>();
        public static Smoker.Test smoker;

        public static void Main(string[] args)
        {
            ProcessEnvironmentVariables();

            ProcessCommandArgs(args);

            ValidateParameters();

            smoker = new Smoker.Test(config.FileList, config.Host);

            // run one test iteration
            if (! config.RunLoop && ! config.RunWeb)
            {
                if (!smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            Task webTask = null;

            // run as a web server
            if (config.RunWeb)
            {
                // use the default web host builder + startup
                IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .UseUrls(string.Format("http://*:4122/"));

                // build the host
                IWebHost host = builder.Build();

                // run the web server
                webTask = host.RunAsync();
            }

            // run tests in config.RunLoop
            if (config.RunLoop)
            {
                Task t;
                CancellationTokenSource ct;

                for (int i = 0; i < config.Threads; i++)
                {
                    ct = new CancellationTokenSource();
                    t = smoker.RunLoop(i, App.config, ct.Token);

                    tokens.Add(ct);
                    tasks.Add(t);
                }
            }

            // wait for web server to complete or ctrl c
            webTask.Wait();
        }

        private static void ValidateParameters()
        {
            // make it easier to pass host
            if (!config.Host.ToLower().StartsWith("http"))
            {
                if (config.Host.ToLower().StartsWith("localhost"))
                {
                    config.Host = "http://" + config.Host;
                }
                else
                {
                    config.Host = string.Format("https://{0}.azurewebsites.net", config.Host);
                }
            }

            if (config.SleepMs < 0)
            {
                config.SleepMs = 0;
            }

            if (config.Threads > 0)
            {
                // set config.RunLoop to true
                config.RunLoop = true;
            }

            if (config.Threads < 0)
            {
                config.Threads = 0;
            }

            // let's not get too crazy
            if (config.Threads > 10)
            {
                config.Threads = 10;
            }

            // add default files
            if (config.FileList.Count == 0)
            {
                config.FileList.Add(defaultInputFile);
                config.FileList.Add("dotnet.json");
            }
        }

        private static void ProcessCommandArgs(string[] args)
        {
            // process the command line args
            if (args.Length > 0)
            {
                if (args[0] == "--help")
                {
                    // display usage

                    Usage();
                    Environment.Exit(0);
                }

                int i = 0;

                while (i < args.Length)
                {
                    // handle web (-w)
                    if (args[i] == "-w")
                    {
                        config.RunWeb = true;
                    }

                    // process all other args in pairs
                    else if (i < args.Length - 1)
                    {
                        // handle host (-h http://localhost:4120/)
                        if (args[i] == "-h")
                        {
                            config.Host = args[i + 1].Trim();
                            i++;
                        }

                        // handle input files (-i inputFile.json input2.json input3.json)
                        else if (i < args.Length - 1 && (args[i] == "-i"))
                        {
                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string file = args[i + 1].Trim();

                                if (!System.IO.File.Exists(file))
                                {
                                    Console.WriteLine("File not found: {0}", file);
                                    Environment.Exit(-1);
                                }

                                config.FileList.Add(file);
                                i++;
                            }
                        }

                        // handle sleep (-s config.SleepMs)
                        else if (args[i] == "-s")
                        {
                            if (int.TryParse(args[i + 1], out config.SleepMs))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine("Invalid sleep (millisecond) paramter: {0}\r\n", args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }

                        // handle config.Threads (-t config.Threads)
                        else if (args[i] == "-t")
                        {
                            if (int.TryParse(args[i + 1], out config.Threads))
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine("Invalid number of config.Threads paramter: {0}\r\n", args[i + 1]);
                                Usage();
                                Environment.Exit(-1);
                            }
                        }
                    }

                    i++;
                }
            }
        }

        private static void ProcessEnvironmentVariables()
        {
            // Get environment variables

            string env = Environment.GetEnvironmentVariable("RUNWEB");
            if (!string.IsNullOrEmpty(env))
            {
                bool.TryParse(env, out config.RunWeb);
            }

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                int.TryParse(env, out config.Threads);
            }

            env = Environment.GetEnvironmentVariable("HOST");
            if (!string.IsNullOrEmpty(env))
            {
                config.Host = env;
            }

            env = Environment.GetEnvironmentVariable("SLEEP");
            if (!string.IsNullOrEmpty(env))
            {
                int.TryParse(env, out config.SleepMs);
            }

            env = Environment.GetEnvironmentVariable("FILES");
            if (!string.IsNullOrEmpty(env))
            {
                // TODO - parse files
            }
        }

        // display the usage text
        private static void Usage()
        {
            Console.WriteLine("Usage: integration-test [-i file1.json [file2.json] [file3.json] ...] [-h hostUrl] [-s sleepMs] [-t numberOfThreads] [-w] [--help]");
        }
    }
}
