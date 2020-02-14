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
        public AdminController(IAccountService accountService)
        {
            AccountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));
        }

        public IAccountService AccountService { get; }

        [HttpGet]
        [Route("account")]
        public IExternalAccount CreateExternalAccount([FromQuery] string id)
        {
            return AccountService.CreateExternalAccount(id);
        }
    }
}
