using System;

namespace CA.Platform.Application.Contracts
{
    public class ResultDto
    {
        public ResultDto(Guid id)
        {
            Id = id;
        }
        
        public Guid Id { get; }
    }
}