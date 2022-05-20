using System;

namespace CircuitBreaker.Abstractions
{

    public class CircuitBreakerModel
    {
        public string Id { get; }
        public string Status { get; set; }
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime NextAttempt { get; set; }

        public CircuitBreakerModel() { }

        public CircuitBreakerModel(
            string id,
            string status = CircuitBreakerStatus.Closed,
            int failureCount = 0,
            int successCount = 0,
            DateTime? nextAttempt = null)
        {
            Id = id;
            Status = status;
            FailureCount = failureCount;
            SuccessCount = successCount;
            NextAttempt = nextAttempt ?? DateTime.UtcNow;
        }

        internal bool Equals(CircuitBreakerModel model)
        {
            return Id == model.Id &&
                Status == model.Status &&
                FailureCount == model.FailureCount &&
                SuccessCount == model.SuccessCount &&
                NextAttempt == model.NextAttempt;
        }

        internal CircuitBreakerModel Clone() 
        {
            return new CircuitBreakerModel(Id, Status, FailureCount, SuccessCount, NextAttempt);
        }
    }
}