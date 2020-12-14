using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SelfHosting.Services;

namespace Logi3plJMS.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public IActionResult Get()
        {
           var jobs = _jobService.GetAll();

            return Ok(jobs);
        }
    }
}
