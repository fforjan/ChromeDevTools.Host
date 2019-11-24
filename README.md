# ChromeDevTools.Host


The library allows to host a chrome dev protocol endpoint into your own .net 4.7.2 application  or a asp.net core application.

You can then use the Chrome DevTools to explore your application. 
The idea is that th ChromeDevTools shoudl explore the domain information and not low level data:
in case of chrome, you do not see the full set of low level TCP info or low-level C++ javascript objects details but 
the network activities or the javascript objects.
You can now do the same with your application !

# Supported feature
- Logging support
- Memory snapshot
- Scripting
- Debugging [virtual scripts](Documentation/VirtualScript.md)

# Demo

### **Step 1** - run the sample you want.
``` 
$ FwkConsoleApp.exe
listening on http://127.0.0.1:12345/
```

### **Step 2** - configure the port in the chrome://inspect window
![Configuration](Documentation/configuration.gif)

### **Step 3** - explore !
![Inspect](Documentation/inspecting.gif)


# Implementation details
This is the counter part of https://github.com/BaristaLabs/chrome-dev-tools-runtime and re-use part of it (JSON definition)

- AspNet.Core is reuse the AspNet Core Middle
- FwkSelfHosted is re-use the HttpListener available in .Net Framework.
