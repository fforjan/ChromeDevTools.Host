# Logging

Log information can be sent to any opened sessions.
There is helper available to help you with your code.

It is recommended to always send all the data as the filtering can be done client side.

```csharp
var sessions = new ChromeProtocolSessions();

// ...
sessions.Error("Merde !!!");
```