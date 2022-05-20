using System;
using System.Threading.Tasks;

namespace CircuitBreaker.Abstractions
{
    public class CircuitBreakerOptions<T>
    {
        public string CircuitBreakerId { get; }    
        public int FailureThreshold { get; }
        public int SuccessThreshold { get; } 
        public int Timeout { get; }
        public Func<Task<T>> Request { get; }
        public Func<Task<T>> Fallback { get; }

        public CircuitBreakerOptions(
            string circuitBreakerId,
            Func<Task<T>> request,
            int failureThreshold = Constants.DefaultFailureThreshold,
            int successThreshold = Constants.DefaultSuccessThreshold,
            int timeout = Constants.DefaultTimeoutToRetry,
            Func<Task<T>> Fallback = null
            ) 
        {
            this.CircuitBreakerId = circuitBreakerId;
            this.Request = request;
            this.FailureThreshold = failureThreshold;
            this.SuccessThreshold = successThreshold;
            this.Timeout = timeout;
            this.Fallback = Fallback;
        }
    }
}