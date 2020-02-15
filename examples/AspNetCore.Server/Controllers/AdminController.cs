using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Services;

namespace AspNetCore.Server.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        public AdminController(IExternalAccountService externalAccountService)
        {
            ExternalAccountService = externalAccountService
                ?? throw new ArgumentNullException(nameof(externalAccountService));
        }

        public IExternalAccountService ExternalAccountService { get; }

        [HttpGet]
        [Route("account/create")]
        public IExternalAccount CreateExternalAccount([FromQuery] string id)
        {
            return ExternalAccountService.Create(id);
        }

        [HttpGet]
        [Route("account/{id:int}")]
        public IExternalAccount GetAccount(int id)
        {
            return ExternalAccountService.GetById(id);
        }
    }
}
