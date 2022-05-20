namespace CircuitBreaker
{
    public static class Constants
    {
        public const int DefaultFailureThreshold = 5;
        public const int DefaultSuccessThreshold = 2;
        public const int DefaultTimeoutToRetry = 10000;
    }
}
