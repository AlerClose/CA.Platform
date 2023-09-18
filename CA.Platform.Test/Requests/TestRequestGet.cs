using MediatR;

namespace CA.Platform.Test;

public class TestRequestGet : IRequest<string>
{
    public string Test { get; set; }
    
    class TestRequestGetHandler: IRequestHandler<TestRequestGet, string>
    {
        public Task<string> Handle(TestRequestGet request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Test);
        }
    }
}