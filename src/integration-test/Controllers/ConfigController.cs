using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
