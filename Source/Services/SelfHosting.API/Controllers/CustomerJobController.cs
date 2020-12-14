using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SelfHosting.Common.Request;
using SelfHosting.Services;

namespace SelfHosting.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CustomerJobController : Controller
    {

        private readonly ICustomerJobService _customJobService;

        public CustomerJobController(ICustomerJobService customJobService)
        {
            _customJobService = customJobService;
        }

        [HttpGet]
        public IActionResult Get()
        {
           var customerJobs =  _customJobService.GetAll();

            return Ok(customerJobs);
        }

        [HttpPost]
        public async Task<dynamic> AssignJob(AssignJobRequest assignJobRequest)
        {
            return await _customJobService.AssignJob(assignJobRequest);
        }
    }
}
