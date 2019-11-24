using System;
using System.Collections.Generic;
using System.Reflection;
using ChromeDevTools.Host.Runtime.Runtime;

namespace ChromeDevTools.Host.Handlers.Runtime
{
    public class PropertyDescriptorCreator
    {
      
        public IEnumerable<PropertyDescriptor> GetProperties(object context)
        {
            foreach (var property in context.GetType().GetProperties())
            {
                if(property.PropertyType == typeof(string))
                {
                    yield return GetStringProperty(context, property);
                }

                PropertyDescriptor value = null;
                try
                {
                    value = GetNumberProperty(context, property);
                }
                catch { }
                if(value != null)
                {
                    yield return value;
                }
            }
        }

        private PropertyDescriptor GetStringProperty(object context, PropertyInfo info)
        {
            return new PropertyDescriptor
            {
                Configurable = false,
                Enumerable = true,
                IsOwn = true,
                Writable = false,
                Name = info.Name,
                Value = RemoteObjectCreator.Create((string)info.GetValue(context))
            };
        }

        private PropertyDescriptor GetNumberProperty(object context, PropertyInfo info)
        {
            return new PropertyDescriptor
            {
                Configurable = false,
                Enumerable = true,
                IsOwn = true,
                Writable = false,
                Name = info.Name,
                Value = RemoteObjectCreator.Create(Convert.ToDouble(info.GetValue(context)))
            };
        }
    }
}
