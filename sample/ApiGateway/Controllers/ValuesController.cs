using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Limited.Gateway.Cache;
using Limited.Gateway.Options;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ICache cache;
        private Route route;

        public ValuesController(ICache _cache, Route _route)
        {
            cache = _cache;
            route = _route;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var items = new List<RouteOption>();

            var baseRoute = new RouteOption()
            {
                SourcePathRegex = @"BaseApi/\w*",
                TargetService = "base",
                TargetPathRegex = @"base/\w*",
                Version = "1.0"
            };

            items.Add(baseRoute);

            var orderRoute = new RouteOption()
            {
                SourcePathRegex = @"OrderApi/\w*",
                TargetService = "Order",
                TargetPathRegex = @"Order/\w*",
                Version = "1.0"
            };
            items.Add(orderRoute);


            await route.Push(items);

            return Ok("111");
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}