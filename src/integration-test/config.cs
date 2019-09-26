using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeliumIntegrationTest
{
    public class Config
    {
        public int SleepMs = 0;
        public int Threads = 0;
        public bool RunLoop = false;
        public bool RunWeb = false;
        public string Host = "http://localhost:4120";
        public readonly List<string> FileList = new List<string>();
    }
}
