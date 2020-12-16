using System;

namespace CA.Platform.Exceptions
{
    public class NotFoundException : Exception
    {
        public Guid Id { get; }

        public Type EntityType { get; }

        public NotFoundException(Guid id, Type type)
        {
            Id = id;
            EntityType = type;
        }
    }
}
