### Overview

This sample application is a regular .AspNetCore application doing an echo system - i.e. write you back what you typed.

It is mainly demonstrating the logging infrastructure.

### Where to start.

Regular services are registered into the SingleSessionProvider.cs:
```C#
    return new ChromeProtocolSession(webSocket, new RuntimeHandler(), new DebuggerHandler(), new ProfilerHandler());
```

The logging system is used into the EchoService.cs itself:

```C#
await Task.WhenAll(ChromeHostExtension.Sessions.ForEach(_ => Extensions.GetService<RuntimeHandler>(_).Log("New Connection")));
```



