using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace HeliumIntegrationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : Controller
    {
        const string logPath = "/home/LogFiles";

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            string val;
            string f;

            SortedList<string, string> files = new SortedList<string, string>();

            if (Directory.Exists(logPath))
            {
                foreach (var p in Directory.GetFiles(logPath, "*.log"))
                {
                    f = Path.GetFileName(p);

                    val = Request.IsHttps ? "https://" : "http://" + Request.Host + Request.Path;

                    if (!val.EndsWith("/"))
                    {
                        val += "/";
                    }

                    files.Add(f, val + f);
                }
            }
            else
            {
                return NotFound("404 " + logPath + " not found");
            }

            return Ok(files.Values);
        }

        [HttpGet("{file}")]
        public IActionResult GetValue(string file)
        {
            Response.ContentType = "text/plain";

            if (Directory.Exists(logPath))
            {
                if (System.IO.File.Exists(Path.Combine(logPath, file)))
                {
                    return Ok(System.IO.File.ReadAllText(Path.Combine(logPath, file)));
                }
                else
                {
                    return NotFound("404 " + Path.Combine(logPath + file) + " not found");
                }
            }
            else
            {
                return NotFound("404 " + logPath + " not found");
            }
        }
    }
}
