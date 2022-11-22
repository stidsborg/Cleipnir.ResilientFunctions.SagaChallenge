using Microsoft.AspNetCore.Mvc;
using OrderWebApi.OrderProcessor;

namespace OrderWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderProcessor.OrderProcessor _orderProcessor;

    public OrderController(OrderProcessor.OrderProcessor orderProcessor)
    {
        _orderProcessor = orderProcessor;
    }
    
    [HttpPost]
    public async Task Post(Order order)
    {
        await _orderProcessor.ProcessOrder(order.OrderId, order);
    }
}