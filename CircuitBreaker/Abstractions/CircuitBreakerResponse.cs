using System;

namespace CircuitBreaker.Abstractions
{

    public class CircuitBreakerResponse<T>
    {
        public T Response { get; set; }
        public Exception Exception { get; set; }
    }
}