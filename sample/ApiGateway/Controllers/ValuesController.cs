using Limited.Gateway;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private RouteTable route;

        public ValuesController(RouteTable _route)
        {
            route = _route;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var items = new List<RouteOption>();

            var baseRoute = new RouteOption()
            {
                SourcePathRegex = @"/a/{everything}",
                TargetService = "base",
                TargetPathRegex = @"/api/{everything}",
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