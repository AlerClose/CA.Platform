using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CA.Platform.Application.Behaviors
{
    class PerfomanceLoggerBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly IUserContext _userContext;

        public PerfomanceLoggerBehavior(ILogger<TRequest> logger, IUserContext userContext)
        {
            _logger = logger;
            _userContext = userContext;
            _timer = new Stopwatch();
        }
        
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _timer.Start();

            var response = await next();
            
            _timer.Stop();

            if (_timer.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning($"Process request {request.GetType().Name} for user {_userContext.GetCurrentUser()?.Login} take {_timer.ElapsedMilliseconds} ms");
            }

            return response;
        }
    }
}