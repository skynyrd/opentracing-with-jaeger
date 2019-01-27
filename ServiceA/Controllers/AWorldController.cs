using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ServiceA.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AWorld : ControllerBase
    {
        [HttpGet("id/{id}")]
        public async Task<ActionResult<dynamic>> Get(string id)
        {
            var b = await GetBObject(id);

            var dummyA = new
            {
                IdOfA = id,
                B = new
                {
                    IdOfB = b.id
                }
            };

            return dummyA;
        }

        private static async Task<dynamic> GetBObject(string id)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:7334")
            };

            var result = await httpClient.GetAsync($"/bworld/id/{id}");

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsAsync<dynamic>();
            }
            
            throw new Exception("uncovered area.");
        }
    }
}