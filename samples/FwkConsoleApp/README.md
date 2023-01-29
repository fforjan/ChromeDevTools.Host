### Overview

This sample application is a regular .net console application calculating th Fibonacci number of i, sleeping for 1 second, increment i and loop.

It will also use logging and allow virtual debugging at different points of the algorithm


### Where to start.

Customized services are registered into the SingleSessionProvider.cs:
```C#
    return new ChromeProtocolSession(webSocket, new MyRuntimeHandler(mainScript), new DebuggerHandler(mainScript, fibonacci), new ProfilerHandler(), new MyHeapProfilerHandler());
```

Notice the usage of ```MyRuntimeHandler```, ```MyHeapProfilerHandler``` and set of scripts provided to ```DebuggerHandler```