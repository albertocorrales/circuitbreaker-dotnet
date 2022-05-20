using CircuitBreaker.Abstractions;

namespace CircuitBreaker.Factory
{
    public interface ICircuitBreakerFactory
    {
        CircuitBreakerInstance<T> CreateCircuitBreaker<T>(CircuitBreakerOptions<T> options);
    }
}