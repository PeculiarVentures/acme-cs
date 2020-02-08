using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Memory.Repositories;
using Xunit;

namespace Repositories
{
    using MemoryAuthorization = PeculiarVentures.ACME.Server.Data.Memory.Models.Authorization;
    using MemoryIdentifier = PeculiarVentures.ACME.Server.Data.Memory.Models.Identifier;

    public class AuthorizationRepositoryTests
    {
        [Fact]
        public void GetById()
        {
            // Init
            var repo = new AuthorizationRepository();

            // Create authz
            var authz = repo.Create();
            authz.Identifier.Type = "dns";
            authz.Identifier.Value = "some.test.com";
            authz.Status = AuthorizationStatus.Pending;
            repo.Add(authz);

            // Get authz by Id
            var authz2 = repo.GetById(1);
            Assert.Equal(1, authz2.Id);
        }

        [Fact]
        public void GetById_Null()
        {
            // Init
            var repo = new AuthorizationRepository();

            // Create authz
            var authz = repo.Create();
            authz.Identifier.Type = "dns";
            authz.Identifier.Value = "some.test.com";
            authz.Status = AuthorizationStatus.Pending;
            repo.Add(authz);

            // Get authz by Id
            var authz2 = repo.GetById(9); // wrong id
            Assert.Null(authz2);
        }

        [Fact]
        public void GetByIdentifier_Null()
        {
            // Init
            var repo = new AuthorizationRepository();

            #region Prepare records

            // Record #1
            repo.Add(new MemoryAuthorization
            {
                Identifier = new MemoryIdentifier
                {
                    Type = "dns",
                    Value = "some.test.com",
                },
                Status = AuthorizationStatus.Invalid,
            });

            // Record #3
            repo.Add(new MemoryAuthorization
            {
                Identifier = new MemoryIdentifier
                {
                    Type = "dns",
                    Value = "some.test2.com",
                },
                Status = AuthorizationStatus.Pending,
            });

            // Record #3
            repo.Add(new MemoryAuthorization
            {
                Identifier = new MemoryIdentifier
                {
                    Type = "dns",
                    Value = "some.test.com",
                },
                Status = AuthorizationStatus.Pending,
            });

            #endregion

            // Get authz by Id
            var identifier = new Identifier
            {
                Type = "DNS", // check case insensitive
                Value = "Some.Test.Com"
            };
            var res = repo.GetByIdentifier(0, identifier); // wrong id
            Assert.Equal(3, res.Id);
        }
    }
}
