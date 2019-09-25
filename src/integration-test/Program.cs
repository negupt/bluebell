using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    public class Program
    {
        static int sleepMs = 0;
        static int threads = 0;
        static bool runWeb = false;
        static string baseUrl = "http://localhost:4120";
        static readonly string defaultInputFile = "integration-test.json";
        static bool loop = false;
        static readonly List<string> fileList = new List<string>();

        public static void Main(string[] args)
        {
            ProcessEnvironmentVariables();

            ProcessCommandArgs(args);

            ValidateParameters();

            Smoker.Test smoker = new Smoker.Test(fileList, baseUrl);

            // run one test iteration
            if (!loop && !runWeb)
            {
                if (!smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            List<Task> tasks = new List<Task>();

            // run as a web server
            if (runWeb)
            {
                // use the default web host builder + startup
                IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

                // set the listen port
                builder.UseUrls(string.Format("http://*:{0}/", 4122));

                // build the host
                IWebHost host = builder.Build();

                // run the web server
                tasks.Add(host.RunAsync());
            }

            // run tests in loop
            if (loop)
            {
                for (int i = 0; i < threads; i++)
                {
                    tasks.Add(smoker.RunLoop(sleepMs));
                }
            }

            // wait for tasks to complete or ctrl c
            Task.WaitAll(tasks.ToArray());
        }

        private static void ValidateParameters()
        {
            // make it easier to pass host
            if (!baseUrl.ToLower().StartsWith("http"))
            {
                if (baseUrl.ToLower().StartsWith("localhost"))
                {
                    baseUrl = "http://" + baseUrl;
                }
                else
                {
                    baseUrl = string.Format("https://{0}.azurewebsites.net", baseUrl);
                }
            }

            if (sleepMs < 0)
            {
                sleepMs = 0;
            }

            if (threads > 0)
            {
                // set loop to true
                loop = true;
            }

            if (threads < 0)
            {
                threads = 0;
            }

            // let's not get too crazy
            if (threads > 10)
            {
                threads = 10;
            }

            // add default file
            if (fileList.Count == 0)
            {
                fileList.Add(defaultInputFile);
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
                        runWeb = true;
                    }

                    // process all other args in pairs
                    else if (i < args.Length - 1)
                    {
                        // handle host (-h http://localhost:4120/)
                        if (args[i] == "-h")
                        {
                            baseUrl = args[i + 1].Trim();
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

                                fileList.Add(file);
                                i++;
                            }
                        }

                        // handle sleep (-s sleepMS)
                        else if (args[i] == "-s")
                        {
                            if (int.TryParse(args[i + 1], out sleepMs))
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

                        // handle threads (-t numberThreads)
                        else if (args[i] == "-t")
                        {
                            if (int.TryParse(args[i + 1], out threads) && threads > 0 && threads <= 10)
                            {
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine("Invalid number of threads paramter: {0}\r\n", args[i + 1]);
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
                bool.TryParse(env, out runWeb);
            }

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                int.TryParse(env, out threads);
            }

            env = Environment.GetEnvironmentVariable("HOST");
            if (!string.IsNullOrEmpty(env))
            {
                baseUrl = env;
            }

            env = Environment.GetEnvironmentVariable("SLEEP");
            if (!string.IsNullOrEmpty(env))
            {
                int.TryParse(env, out sleepMs);
            }

            env = Environment.GetEnvironmentVariable("FILES");
            if (!string.IsNullOrEmpty(env))
            {
                // TODO - parse files
            }

            // TODO - remove once parse works
            fileList.Add(defaultInputFile);
            fileList.Add("dotnet.json");
        }

        // display the usage text
        private static void Usage()
        {
            Console.WriteLine("Usage: integration-test [-i file1.json [file2.json] [file3.json] ...] [-h hostUrl] [-s sleepMS] [-t numberOfThreads] [-w] [--help]");
        }
    }
}
