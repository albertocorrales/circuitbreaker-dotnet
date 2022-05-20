using System.Threading.Tasks;

namespace CircuitBreaker.Abstractions
{
    public interface ICircuitBreakerRepository
    {
        Task<CircuitBreakerModel> GetById(string id);
        Task Upsert(CircuitBreakerModel obj);
    }
}