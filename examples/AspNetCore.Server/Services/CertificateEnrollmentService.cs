using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AspNetCore.Server.Services
{
    public class CertificateEnrollmentService : BaseService, ICertificateEnrollmentService
    {
        public CertificateEnrollmentService(IOptions<ServerOptions> options)
            : base(options)
        {
        }

        private static long LongRandom(long min, long max, Random rand)
        {
            long result = rand.Next((int)(min >> 32), (int)(max >> 32));
            result = (result << 32);
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            result = result | rand.Next((int)min, (int)max);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            return result;
        }

        public static X509Certificate2 GenerateCertificate(
            string issuer, string subject,
            AsymmetricAlgorithm issuerPrivate,
            AsymmetricAlgorithm subjectPublic)
        {
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(
                PkcsObjectIdentifiers.Sha256WithRsaEncryption.ToString(),
                DotNetUtilities.GetRsaKeyPair((RSA)issuerPrivate).Private);

            var certGenerator = new X509V3CertificateGenerator();
            certGenerator.SetIssuerDN(new X509Name(issuer));
            if (!string.IsNullOrEmpty(subject))
            {
                certGenerator.SetSubjectDN(new X509Name(subject));
            }
            else
            {
                certGenerator.SetSubjectDN(X509Name.GetInstance(Asn1Object.FromByteArray(new byte[] { 0x30, 0x00 })));
            }
            certGenerator.SetSerialNumber(BigInteger.ValueOf(LongRandom(0, 1000000000, new Random())));
            certGenerator.SetNotAfter(DateTime.UtcNow.AddHours(1));
            certGenerator.SetNotBefore(DateTime.UtcNow);
            certGenerator.SetPublicKey(DotNetUtilities.GetRsaPublicKey((RSA)subjectPublic));
            var rawData = certGenerator.Generate(signatureFactory).GetEncoded();
            return new X509Certificate2(rawData);
        }

        public async Task<X509Certificate2> Enroll(IOrder order, Pkcs10CertificateRequest request)
        {
            return await Task.Run(() =>
            {
                var caCert = Options.ExtraCertificateStorage[1];
                return GenerateCertificate(caCert.Subject, request.Subject, caCert.PrivateKey, request.PublicKey.Key);
            });
        }

        public Task Revoke(IOrder order, RevokeReason reason)
        {
            return Task.Run(() =>
            {
                // nothing
            });
        }
    }
}
