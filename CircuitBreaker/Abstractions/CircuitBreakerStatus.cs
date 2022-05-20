namespace CircuitBreaker.Abstractions
{

    public static class CircuitBreakerStatus
    {
        public const string Open = "Open";
        public const string Half = "Half";
        public const string Closed = "Closed";
    }
}