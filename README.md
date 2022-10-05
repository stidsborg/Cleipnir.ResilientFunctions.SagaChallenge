# Cleipnir's ResilientFunctions Saga Challenge

Hey there and welcome!

Imagine you have just been tasked with completing the implementation of an order processing flow - see [OrderProcessor.cs](./OrderProcessor.cs).

The current order-flow has been running poorly in production and customers have received ordered products without their credit card ever being charged.

The company urgently needs the issue to be rectified and have decided to use Cleipnir’s [Resilient Functions](https://github.com/stidsborg/Cleipnir.ResilientFunctions) framework to ensure order processing is retried if it crashes during execution and flag problematic orders for manual handling.

Using Resilient Functions automatically retries crashed order invocations and provides you with the ability to design a custom scrapbook type for keeping track of the order’s progress. The latter is really useful when an order processing crashes and is restarted.

You can add all the properties to the scrapbook type you need to ensure the order flow executes correctly. Also a scrapbook can be saved at any time by invoking its Save-method - thereby ensuring that its state will be available from that point onwards despite crashes and restarts. 

You have 2 requirements:
1. The payment provider api accepts a client generated transaction id for all its endpoints. 
You must ensure that the same id is used for the same order despite the order-flow crashing and restarting.
2. The logistics service responsible for shipping products to customers must be called at most once.
If the order flow crashes while waiting for a reply from the logistics service the order must be flagged for manual handling by throwing an exception.

A friendly colleague has already created several tests asserting if the implementation violates the requirements. 

To get started just clone this repository and make all the tests light green.

Best of luck!
