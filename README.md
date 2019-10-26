# ChromeDevTools.Host
This is the counterpart of 

The library allows to host a chrome dev protocol endpoint into your own .net 4.7.2 application  or a asp.net core application.

You can then use the Chrome DevTools to explore your application. 
The idea is that th ChromeDevTools shoudl explore the domain information and not low level data:
in case of chrome, you do not see the full set of low level TCP info or low-level C++ javascript objects details but 
the network activities or the javascript objects.
You can now do the same with your application !

