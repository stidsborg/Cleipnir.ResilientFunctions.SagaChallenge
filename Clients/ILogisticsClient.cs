using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaChallenge.Clients;

public interface ILogisticsClient
{
    Task ShipProducts(Guid customerId, IEnumerable<Guid> productIds);
}