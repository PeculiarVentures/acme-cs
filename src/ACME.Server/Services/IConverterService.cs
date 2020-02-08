﻿using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IConverterService
    {
        Challenge ToChallenge(IChallenge data);
        Error ToError(IError data);
        Account ToAccount(IAccount data);
        Authorization ToAuthorization(IAuthorization data);
        Order ToOrder(IOrder data);
    }
}
