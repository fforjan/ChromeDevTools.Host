namespace ChromeDevTools.Host.Handlers.Runtime
{
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class RuntimeHandler : IRuntimeHandler
    {
        private ChromeProtocolSession session;

        private PropertyDescriptorCreator propertyDescriptorCreator = new PropertyDescriptorCreator();

        private Dictionary<string, List<object>> localObjects = new Dictionary<string, List<object>>();

        public IDisposable AllocateLocalObject(string id, object context)
        {
            if(!localObjects.ContainsKey(id))
            {
                localObjects[id] = new List<object>();
            }

            localObjects[id].Add(context);
            return new LocalContextDisposable(this, id);
        }


        public virtual void Register(ChromeProtocolSession session)
        {
            this.IsEnable = false;


            this.session = session;
            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
            session.RegisterCommandHandler<DisableCommand>(DisableCommand);
            session.RegisterCommandHandler<GetHeapUsageCommand>(GetHeapUsageCommand);
            session.RegisterCommandHandler<EvaluateCommand>(EvaluateCommand);

            session.RegisterCommandHandler<GetPropertiesCommand>(GetPropertiesCommand);
        }

        public virtual bool IsEnable { get; protected set; }

        public async Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            if (!this.IsEnable)
            {
                this.IsEnable = true;
                await session.SendEvent(new ExecutionContextCreatedEvent
                {
                    Context = new ExecutionContextDescription
                    {
                        Id = Context,
                        Origin = "",
                        Name = "virtual context",
                        AuxData = new
                        {
                            isDefault = true
                        }
                    }
                });
            }

            return new EnableCommandResponse();
        }

        public Task<ICommandResponse<GetPropertiesCommand>> GetPropertiesCommand(GetPropertiesCommand command)
        {
            return Task.Run<ICommandResponse<GetPropertiesCommand>>(() =>
            {
                var result = new GetPropertiesCommandResponse();
                if(this.localObjects.TryGetValue(command.ObjectId, out var localObject))
                {
                    result.Result = propertyDescriptorCreator.GetProperties(localObject.Last()).ToArray();        
                }
                else
                {
                    result.Result = Array.Empty<PropertyDescriptor>();
                }
                return result;
            });
        }



        public Task<ICommandResponse<DisableCommand>> DisableCommand(DisableCommand command)
        {
            this.IsEnable = false;
            return Task.FromResult<ICommandResponse<DisableCommand>>(new DisableCommandResponse());
        }

        public Task<ICommandResponse<GetHeapUsageCommand>> GetHeapUsageCommand(GetHeapUsageCommand command)
        {
            var usage = GetHeapUsage();

            return Task.FromResult<ICommandResponse<GetHeapUsageCommand>>(new GetHeapUsageCommandResponse
            {
                TotalSize = usage.TotalSize,
                UsedSize = usage.UsedSize
            });
        }

        public Task<ICommandResponse<EvaluateCommand>> EvaluateCommand(EvaluateCommand command)
        {
            return Task.FromResult<ICommandResponse<EvaluateCommand>>(new EvaluateCommandResponse
            {
                Result = Evaluate(command.Expression)
            });
        }

        protected virtual RemoteObject Evaluate(string expr)
        {
            return RemoteObjectCreator.Create("Not implemented");
        }

        protected virtual (double TotalSize, double UsedSize) GetHeapUsage()
        {
            return (Process.GetCurrentProcess().PrivateMemorySize64, GC.GetTotalMemory(false));
        }

        public Task Log(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "log"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task Warning(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "warning"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task Error(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "error"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task Debug(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "debug"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private ConsoleAPICalledEvent GetLogEvent(string logMessage, string level)
        {
            var logEvent = new ConsoleAPICalledEvent
            {
                Type = level,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ExecutionContextId = Context,
                Args = new RemoteObject[]
                {
                   RemoteObjectCreator.Create(logMessage)
                }
            };

            return logEvent;
        }

        public int Context { get { return 1; } }

        private class LocalContextDisposable : IDisposable
        {
            private readonly RuntimeHandler handler;
            private readonly string name;

            public LocalContextDisposable(RuntimeHandler handler, string name)
            {
                this.handler = handler;
                this.name = name;
            }
            public void Dispose()
            {
                this.handler.localObjects[name].RemoveAt(this.handler.localObjects[name].Count - 1 );
            }
        }
    }
}