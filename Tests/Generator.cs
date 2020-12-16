using System;
using CA.Platform.Entities;

namespace CA.Platform.Tests
{
    public static class Generator
    {
        public static T Get<T>()
        {
            var type = typeof(T);

            int counter = new Random(DateTime.Now.Millisecond).Next();
            
            if (type == typeof(string))
                return (T) Convert.ChangeType("String" + counter, type);
            
            if (type == typeof(int))
                return (T) Convert.ChangeType(counter, type);

            if (type == typeof(DateTime))
            {
                var date = DateTime.Now;
                return (T) Convert.ChangeType(
                    DateTime.Today.AddHours(date.Hour).AddMinutes(date.Minute).AddSeconds(date.Second), type);
            }

            T result = Activator.CreateInstance<T>();

            foreach (var property in type.GetProperties())
            {
                if (!property.CanWrite)
                    continue;
                
                if (property.PropertyType == typeof(string))
                    property.SetValue(result, property.Name + counter);
                
                if (property.Name == nameof(BaseObject.Id) && property.PropertyType == typeof(Guid))
                    property.SetValue(result, Guid.NewGuid());
            }

            return result;
        }
    }
}