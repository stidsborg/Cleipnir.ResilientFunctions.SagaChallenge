using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaChallenge1.Clients;

public interface IEmailClient
{
    Task SendOrderConfirmation(Guid customerId, IEnumerable<Guid> productIds);
}