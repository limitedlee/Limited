using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace DownstreamService.Controllers
{
    /// <summary>
    /// values
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// GET api/values/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            Console.WriteLine($"{DateTime.Now.ToString()}----{id}");

            return Ok(id);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// POST api/values
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public ActionResult<string> Post([FromBody] p str)
        {
            Console.WriteLine($"{DateTime.Now.ToString()}----{str.ToString()}");

            return "ok";
        }

        /// <summary>
        /// 测试SwaggerResponse
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("x")]
        [SwaggerResponse(200, Type = typeof(p))]
        public dynamic GetItems()
        {
            return BadRequest("请选择");
        }
    }

    public class p
    {
        public string mobile { get; set; }

        public string vc { get; set; }
    }
}
