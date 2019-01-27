using System;
using System.Threading;
using JaegerWrapper;
using Microsoft.AspNetCore.Mvc;
using OpenTracing.Tag;

namespace ServiceB.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BWorldController : ControllerBase
    {
        private readonly ITraceBuilder _traceBuilder;

        public BWorldController(ITraceBuilder traceBuilder)
        {
            _traceBuilder = traceBuilder;
        }
        
        [HttpGet("id/{id}")]
        public ActionResult<dynamic> Get(string id)
        {
            if (int.TryParse(id, out var parsed))
            {
                if (parsed > 10)
                {
                    LongRunningProcess();
                }
                return new {id};
            }
            
            throw new Exception("Id should only contain numbers.");
        }

        private void LongRunningProcess()
        {
            _traceBuilder
                .WithSpanName("LongRunningProcess")
                .WithTag(new StringTag("exampleTag"), "exampleValue")
                .TraceIt(() =>
                {
                    Thread.Sleep(1000);
                });
        }
    }
}