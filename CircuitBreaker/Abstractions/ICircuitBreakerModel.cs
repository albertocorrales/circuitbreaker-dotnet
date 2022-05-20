using System;

namespace CircuitBreaker.Abstractions
{
    public interface ICircuitBreakerModel
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime NextAttempt { get; set; }
    }
}
