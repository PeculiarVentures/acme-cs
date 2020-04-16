using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Services;
using PeculiarVentures.ACME.Web;

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
            var q = GetQuery();
            return ExternalAccountService.Create(id);
        }

        [HttpGet]
        [Route("account/{id:int}")]
        public IExternalAccount GetAccount(int id)
        {
            var q = GetQuery();
            return ExternalAccountService.GetById(id);
        }

        private Dictionary<string, string[]> GetQuery()
        {
            var query = new Query();
            if (Request.QueryString.HasValue)
            {
                foreach (var item in Request.Query)
                {
                    query.Add(item.Key, item.Value);
                }
            }
            return query;
        }
    }
}
