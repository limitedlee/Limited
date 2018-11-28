using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Limited.Gateway.Cache;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ICache cache;

        public ValuesController(ICache _cache)
        {
            cache = _cache;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var obj = cache.Exists("123");
            obj.Wait();

            var a = obj.Result;
            
            return Ok("123");
        }
    }
}