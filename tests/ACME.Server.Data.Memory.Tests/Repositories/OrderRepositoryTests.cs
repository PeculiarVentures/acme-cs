using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Memory.Repositories;
using Xunit;

namespace Repositories
{
    using MemoryOrder = PeculiarVentures.ACME.Server.Data.Memory.Models.Order;
    using MemoryAuthorization = PeculiarVentures.ACME.Server.Data.Memory.Models.Authorization;
    using MemoryIdentifier = PeculiarVentures.ACME.Server.Data.Memory.Models.Identifier;

    public class OrderRepositoryTests
    {

        [Fact]
        public void GetByIdentifiers()
        {
            // init
            var repo = new OrderRepository();

            #region Prepare data
            // Records #1
            repo.Add(new MemoryOrder
            {
                Authorizations = new MemoryAuthorization[]
                {
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server1"),
                    },
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server2"),
                    },
                },
            });

            // Records #2
            repo.Add(new MemoryOrder
            {
                Authorizations = new MemoryAuthorization[]
                {
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server1"),
                    },
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server2"),
                    },
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server3"),
                    },
                },
            });

            // Records #3
            repo.Add(new MemoryOrder
            {
                Authorizations = new MemoryAuthorization[]
                {
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server1"),
                    },
                },
            });

            // Records #4
            repo.Add(new MemoryOrder
            {
                Authorizations = new MemoryAuthorization[]
                {
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server1"),
                    },
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server2"),
                    },
                },
            });

            // Records #5
            repo.Add(new MemoryOrder
            {
                Authorizations = new MemoryAuthorization[]
                {
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server1"),
                    },
                    new MemoryAuthorization
                    {
                        Identifier = new MemoryIdentifier("dns", "server3"),
                    },
                },
            });
            #endregion

            var res = repo.GetByIdentifiers(0, new Identifier[] {
                new Identifier("dns","server2"), // change order for test
                new Identifier("dns","server1"),
            });

            Assert.Equal(4, res.Id);
        }

    }
}
