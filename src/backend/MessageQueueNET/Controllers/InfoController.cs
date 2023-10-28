using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Services.Abstraction;
using MessageQueueNET.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MessageQueueNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IMessageQueueApiVersionService _apiVersionService;
        public InfoController(IMessageQueueApiVersionService apiVersionService)
        {
            _apiVersionService = apiVersionService;
        }

        [HttpGet]
        [Route("")]
        public InfoResult Info()
        {
            var result = new InfoResult();

            try
            {
                result.Version = _apiVersionService.Version;
            }
            catch (Exception ex)
            {
                result.AddExceptionMessage(ex);
            }

            return result;
        }
    }
}
