using CircuitBreaker.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CircuitBreaker
{
    public class CircuitBreakerInstance<T> : ICircuitBreaker<T>
    {
        private readonly int _timeout;
        private readonly int _successThreshold;
        private readonly int _failureThreshold;
        private readonly Func<Task<T>> _fallback;
        private readonly Func<Task<T>> _request;
        private readonly ICircuitBreakerRepository _repository;
        private readonly ILogger _logger;
        private CircuitBreakerModel _storedCircuitBreakerModel = null;

        public CircuitBreakerModel CircuitBreakerModel { get; private set; }

        public CircuitBreakerInstance(CircuitBreakerOptions<T> options, ICircuitBreakerRepository repository, ILogger logger)
        {
            _logger = logger;
            _repository = repository;

            _failureThreshold = options.FailureThreshold;
            _successThreshold = options.SuccessThreshold;
            _fallback = options.Fallback;
            _request = options.Request;
            _timeout = options.Timeout;

            CircuitBreakerModel = new CircuitBreakerModel(options.CircuitBreakerId);
        }

        public async Task<T> Fire()
        {
            try
            {
                _storedCircuitBreakerModel = await _repository.GetById(CircuitBreakerModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting circuit breaker {CircuitBreakerModel.Id} from storage");
            }

            if (_storedCircuitBreakerModel == null)
            {
                try
                {
                    await _repository.Upsert(CircuitBreakerModel);
                    _storedCircuitBreakerModel = CircuitBreakerModel.Clone();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating circuit breaker {CircuitBreakerModel.Id} in storage");
                }
            }
            else
            {
                CircuitBreakerModel = _storedCircuitBreakerModel.Clone();
            }

            if (CircuitBreakerModel.Status == CircuitBreakerStatus.Open)
            {
                if (CircuitBreakerModel.NextAttempt > DateTime.UtcNow)
                {
                    return _fallback != null ?
                        await TryFallback() :
                        throw new Exception("Circuit breaker status: OPEN");
                }

                Half();
            }

            try
            {
                var response = await _request.Invoke();
                return await Success(response);
            }
            catch (Exception ex)
            {
                return await Fail(ex);
            }
        }

        private void Open()
        {
            CircuitBreakerModel.Status = CircuitBreakerStatus.Open;
            CircuitBreakerModel.NextAttempt = DateTime.UtcNow.AddMilliseconds(_timeout);
        }

        private void Close()
        {
            CircuitBreakerModel.SuccessCount = 0;
            CircuitBreakerModel.FailureCount = 0;
            CircuitBreakerModel.Status = CircuitBreakerStatus.Closed;
        }

        private void Half()
        {
            CircuitBreakerModel.Status = CircuitBreakerStatus.Half;
        }

        private async Task<T> Success(T response)
        {
            if (CircuitBreakerModel.Status == CircuitBreakerStatus.Half)
            {
                CircuitBreakerModel.SuccessCount++;
                if (CircuitBreakerModel.SuccessCount > _successThreshold)
                {
                    Close();
                }
            }

            CircuitBreakerModel.FailureCount = 0;
            try
            {
                if (_storedCircuitBreakerModel != null && !_storedCircuitBreakerModel.Equals(CircuitBreakerModel))
                {
                    await _repository.Upsert(CircuitBreakerModel);
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating circuit breaker {CircuitBreakerModel.Id}");
            }
            return response;
        }

        private async Task<T> Fail(Exception exception)
        {
            CircuitBreakerModel.FailureCount++;
            if (CircuitBreakerModel.FailureCount >= _failureThreshold)
            {
                Open();
            }

            try
            {
                if (_storedCircuitBreakerModel != null && !_storedCircuitBreakerModel.Equals(CircuitBreakerModel))
                {
                    await _repository.Upsert(CircuitBreakerModel);
                }   
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating circuit breaker {CircuitBreakerModel.Id}");
            }

            return _fallback != null ?
                await TryFallback() :
                throw exception;
        }

        private async Task<T> TryFallback()
        {
            return _fallback != null ?
                await _fallback.Invoke() :
                default(T);
        }
    }
}