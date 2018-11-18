using System.Threading.Tasks;

namespace Janus.Core.Algorithms
{
    public interface IRouteAlgorithm<T>
    {
        Task<T> AddRoutedEntity(T routedEntity);
        Task<T> InsertRoutedEntity(T routedEntity, int index);
        Task RemoveRoutedEntity(T routedEntity);
        Task RemoveRoutedEntityAt(int index);
        Task <T> GetNext();
    }
}