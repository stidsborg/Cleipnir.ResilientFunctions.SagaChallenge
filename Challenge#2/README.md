# Challenge #2

Hey again!

Okay, so the solution you created back in the first challenge is functioning really well and everybody are really happy. 

Actually, the solution is so successful that other companies want to use the order-processing functionality. 

Your tech lead has added a Brand-enum to the source code which lists the different new companies (including the existing one AbcLavpris) that want to use the platform.

The next step is to add a Brand property to an order, as the brand needs to be passed on to some of the different external services.

However, a critical thing to note:
* There are always flowing orders through the system. As such when a deployment occurs some order will be partly processed and will be picked up by the framework and reprocessed with the newly deployed code.

It is emphasized the "existing" orders must be handled correctly by the new version of the code. All existing order can be assigned the brand AbcLavpris. 

A friendly colleague has created a console-application which you can use to verify you new order processing logic.

Best of luck!
