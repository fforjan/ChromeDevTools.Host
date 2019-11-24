using System;
using System.Collections.Generic;
using System.Reflection;
using ChromeDevTools.Host.Runtime.Runtime;

namespace ChromeDevTools.Host.Handlers.Runtime
{
    public class PropertyDescriptorCreator
    {

        /// <summary>
        /// Return a list of descriptors from a context
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public IEnumerable<PropertyDescriptor> GetObjectDescriptors(object @object)
        {
            // Check for property first
            foreach (var property in @object.GetType().GetProperties())
            {
                if(property.PropertyType == typeof(string))
                {
                    yield return GetImmutableStringProperty(property.Name, (string)property.GetValue(@object));
                }

                var value  = TryGetImmutableNumberProperty(property.Name, property.GetValue(@object));            
                if(value != null)
                {
                    yield return value;
                }
            }

            // or fields
            foreach (var field in @object.GetType().GetFields())
            {
                if (field.FieldType == typeof(string))
                {
                    yield return GetImmutableStringProperty(field.Name, (string)field.GetValue(@object));
                }

                var value = TryGetImmutableNumberProperty(field.Name, field.GetValue(@object));
                
                if (value != null)
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Create an immutabe string property
        /// </summary>
        private PropertyDescriptor GetImmutableStringProperty(string name, string value)
        {
            return new PropertyDescriptor
            {
                Configurable = false,
                Enumerable = true,
                IsOwn = true,
                Writable = false,
                Name = name,
                Value = RemoteObjectCreator.Create(value)
            };
        }

       /// <summary>
       /// Try to create an immutable number property for the object
       /// </summary>
       /// <param name="name">property name</param>
       /// <param name="value">generic value</param>
       /// <returns>null if the value cannot converted to double</returns>
        private PropertyDescriptor TryGetImmutableNumberProperty(string name, object value)
        {  
            if(value == null) { return null; }

            try
            {
                return new PropertyDescriptor
                {
                    Configurable = false,
                    Enumerable = true,
                    IsOwn = true,
                    Writable = false,
                    Name = name,
                    Value = RemoteObjectCreator.Create(Convert.ToDouble(value))
                };
            }
            catch
            {
                return null; 
            }
            
        }
    }
}
