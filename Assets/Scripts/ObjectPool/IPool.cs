namespace Retrover.ObjectPool
{
    public interface IPool<T> where T : IPoolable
    {
        void Push(T t);
    }
}
