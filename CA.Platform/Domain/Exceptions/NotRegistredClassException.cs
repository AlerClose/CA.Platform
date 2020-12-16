using System;

namespace CA.Platform.Exceptions
{
    public class NotRegistredClassException: Exception
    {
        public Type EntityType { get; }
        
        public NotRegistredClassException(Type type)
        {
            EntityType = type;
        }
    }
}