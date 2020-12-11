using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SelfHosting.Services;

namespace SelfHosting.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CustomerJobController : Controller
    {

        private readonly ICustomJobService _customJobService;

        public CustomerJobController(ICustomJobService customJobService)
        {
            _customJobService = customJobService;
        }

        [HttpGet]
        public IActionResult Get()
        {
           var customerJobs =  _customJobService.GetAll();

            return Ok(customerJobs);
        }
    }
}
