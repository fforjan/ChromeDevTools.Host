using System;
using System.Collections.Generic;
using System.Reflection;
using ChromeDevTools.Host.Runtime.Runtime;

namespace ChromeDevTools.Host.Handlers.Runtime
{
    public class PropertyDescriptorCreator
    {
        public Dictionary<Type, Func<object, PropertyInfo, PropertyDescriptor>> creator;

        public PropertyDescriptorCreator()
        {
            creator = new Dictionary<Type, Func<object, PropertyInfo, PropertyDescriptor>>
            {
                { typeof(string), this.GetStringProperty },
                { typeof(float), this.GetNumberProperty },
                { typeof(int), this.GetNumberProperty },
                { typeof(long), this.GetNumberProperty },
                { typeof(short), this.GetNumberProperty },
                { typeof(double), this.GetNumberProperty },
                { typeof(byte), this.GetNumberProperty },

                { typeof(uint), this.GetNumberProperty },
                { typeof(ulong), this.GetNumberProperty },
                { typeof(ushort), this.GetNumberProperty }, 
            };
        }

        public IEnumerable<PropertyDescriptor> GetProperties(object context)
        {
            foreach (var property in context.GetType().GetProperties())
            {
                if (creator.TryGetValue(property.PropertyType, out var  propertyCreator))
                {
                    yield return propertyCreator(context, property);
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
