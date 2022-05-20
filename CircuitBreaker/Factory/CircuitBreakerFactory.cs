using CircuitBreaker.Factory;
using Microsoft.Extensions.Logging;

namespace CircuitBreaker.Abstractions
{
    public class CircuitBreakerFactory : ICircuitBreakerFactory
    {
        private readonly ILogger _logger;
        private readonly ICircuitBreakerRepository _repository;

        public CircuitBreakerFactory(ICircuitBreakerRepository repository, ILoggerFactory logger)
        {
            _repository = repository;
            _logger = logger.CreateLogger(GetType().Name);
        }

        public CircuitBreakerInstance<T> CreateCircuitBreaker<T>(CircuitBreakerOptions<T> options)
        {
            return new CircuitBreakerInstance<T>(options, _repository, _logger);
        }
    }
}
