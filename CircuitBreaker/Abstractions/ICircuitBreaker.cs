using System.Threading.Tasks;

namespace CircuitBreaker.Abstractions
{
    public interface ICircuitBreaker<T>
    {
        Task<T> Fire();
    }
}