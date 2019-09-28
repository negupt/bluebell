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
    public class RunTestController : Controller
    {
        [HttpGet()]
        public async Task<string> Runtest()
        {
            Response.ContentType = "text/plain";

            return await App.smoker.RunFromWebRequest(App.tokens.Count);
        }
    }
}
