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
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {

            var nodes = new List<CacheNode<string>>();

            for (var m = 0; m < 50; m++)
            {
                var node = new CacheNode<string>()
                {
                    CacheTime = new TimeSpan(0, 10, 0),
                    Data = "Hi,li fa sheng",
                    Key = "baseApi-member-id-" + m
                };

                nodes.Add(node);
            }


            var strs = new List<string>();
            for (var m = 10; m < 15; m++)
            {
                strs.Add("baseApi-member-id-" + m);
            }

            var list = await cache.Get(strs);

             //await cache.Set(nodes);

            //var json = await cache.Get("baseApi/member/id");

            return Ok("111");
        }
    }
}