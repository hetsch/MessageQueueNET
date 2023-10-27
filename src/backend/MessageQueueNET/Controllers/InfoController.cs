using MessageQueueNET.Client.Models;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using System;
using MessageQueueNET.Extensions;

namespace MessageQueueNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IAppVersionService _appVersionService;
        public InfoController(IAppVersionService appVersionService)
        {
            _appVersionService = appVersionService;
        }

        [HttpGet]
        [Route("")]
        public InfoResult Info()
        {
            var result = new InfoResult();

            try
            {
                result.Version = new System.Version(_appVersionService.Version);
            }
            catch (Exception ex)
            {
                result.AddExceptionMessage(ex);
            }

            return result;
        }
    }
}
