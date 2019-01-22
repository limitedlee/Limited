using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DownstreamService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            Console.WriteLine($"{DateTime.Now.ToString()}----{id}");

            return Ok(id);
        }

        [Route("list")]
        [HttpGet]
        public dynamic List()
        {
            List<string> items = new List<string>();
            items.Add("aaaaa");
            items.Add("bbbbb");
            items.Add("vvvvv");
            items.Add("cccccc");
            items.Add("dddddd");

            Response.Headers.Add("X-Pagination-PageCount", "5");
            return items;
        }

        // POST api/values
        [HttpPost]
        [Route("login")]
        public ActionResult<string> Post([FromBody] p str)
        {
            Console.WriteLine($"{DateTime.Now.ToString()}----{str.ToString()}");

            return "ok";
        }
    }

    public class p
    {
        public string mobile { get; set; }

        public string vc { get; set; }
    }
}
