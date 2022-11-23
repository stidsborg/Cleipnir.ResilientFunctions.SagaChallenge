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
    
    /// <summary>
    /// Submit new order for processing
    /// </summary>
    /// <param name="order"></param>
    [HttpPost]
    public async Task Post(Order order)
    {
        await _orderProcessor.ProcessOrder(order.OrderId, order);
    }

    /// <summary>
    /// Re-run order processing for provided order number
    /// </summary>
    /// <param name="orderNumber"></param>
    [HttpPut]
    [Route("{orderNumber}/")]
    public async Task<IActionResult> Put(string orderNumber)
    {
        var controlPanel = await _orderProcessor.ControlPanels.For(orderNumber);
        if (controlPanel == null)
            return NotFound();
        
        await controlPanel.ReInvoke();
        return Ok();
    }
}