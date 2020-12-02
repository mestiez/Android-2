using AndroidServer.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiscordController : ControllerBase
    {
        private readonly AndroidService android;

        public DiscordController(AndroidService android)
        {
            this.android = android;
        }
    }
}
