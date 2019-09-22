using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    // integration test for testing Helium (or any REST API)
    partial class App
    {
        // main entry point
        public static async Task Main(string[] args)
        {
            int sleepMs = 0;
            int threads = 1;
            bool runWeb = false;
            string baseUrl = "http://localhost:4120";
            string defaultInputFile = "integration-test.json";
            bool loop = false;
            List<string> fileList = new List<string>();

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

            env = Environment.GetEnvironmentVariable("THREADS");
            if (!string.IsNullOrEmpty(env))
            {
                int.TryParse(env, out threads);
            }

            // process the command line args
            if (args.Length > 0)
            {
                if (args[0] == "--help")
                {
                    // display usage

                    Usage();
                    return;
                }

                int i = 0;

                while (i < args.Length)
                {
                    // handle web (-w)
                    if (args[i] == "-w")
                    {
                        runWeb = true;
                    }


                    // process all of the args in pairs
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
                            if (int.TryParse(args[i + 1], out sleepMs) && sleepMs > 0)
                            {
                                // set loop to true
                                loop = true;
                                i++;
                            }
                            else
                            {
                                // exit on error
                                Console.WriteLine("Invalid sleep (millisecond) paramter: {0}\r\n", args[i + 1]);
                                Usage();
                                return;
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
                                return;
                            }
                        }
                    }

                    i++;
                }
            }

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

            if (threads < 1)
            {
                threads = 1;
            }

            if (threads > 10)
            {
                threads = 10;
            }

            Smoker.Test smoker = new Smoker.Test(fileList, baseUrl);

            if (!loop && ! runWeb)
            {
                if (!smoker.Run().Result)
                {
                    Environment.Exit(-1);
                }

                return;
            }

            if (runWeb)
            {
                // start the web server
                Tiny.Web host = new Tiny.Web();

                List<Task> tasks = new List<Task>();

                if (loop)
                {
                    for (int i = 0; i < threads; i++)
                    {
                        tasks.Add(smoker.RunLoop(sleepMs));
                    }
                }

                await host.RunAsync(16, 4120);

                return;
            }

            if (fileList.Count == 0)
            {
                fileList.Add(defaultInputFile);
            }

            await smoker.RunLoop(sleepMs);
        }

        // display the usage text
        private static void Usage()
        {
            Console.WriteLine("Usage: integration-test [-i file1.json [file2.json] [file3.json] ...] [-h hostUrl] [-s sleepMS] [-t numberOfThreads] [-w] [--help]");
        }
    }
}
