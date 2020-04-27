
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PeculiarVentures.ACME.Server.Services
{
  public interface ICertificateEnrollmentService
  {
    Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request);
    Task Revoke(IOrder order, Protocol.RevokeReason reason);
  }
}
