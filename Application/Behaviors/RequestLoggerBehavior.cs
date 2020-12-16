using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace CA.Platform.Application.Behaviors
{
    class RequestLoggerBehavior<T>: IRequestPreProcessor<T>
    {
        private readonly ILogger<T> _logger;
        private readonly IUserContext _context;

        public RequestLoggerBehavior(ILogger<T> logger, IUserContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        public Task Process(T request, CancellationToken cancellationToken)
        {
            string logMessage = $"Request {typeof(T).Name} {_context.GetCurrentUser()?.Login}";
            
            _logger.LogTrace(logMessage);
            
            return Task.CompletedTask;
        }
    }
}