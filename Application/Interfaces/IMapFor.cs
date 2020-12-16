using AutoMapper;

namespace CA.Platform.Application.Interfaces
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile);
    }
}