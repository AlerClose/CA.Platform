using AutoMapper;
using CA.Platform.Application.Interfaces;

namespace CA.Platform.Application.Contracts
{
    public abstract class BaseDto<T> : IMapFrom<T>
    {
        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap(typeof(T), GetType());
        }
    }
}