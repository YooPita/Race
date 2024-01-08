using System.Collections.Generic;
using UnityEngine;

namespace Retrover.ObjectPool
{
    public abstract class MonoPool<T> : MonoBehaviour, IPool<T> where T : IPoolable
    {
        [SerializeField] private GameObject _prefab;
        private Stack<T> _stack = new();

        public void Push(T t)
        {
            t.ReturnToPool();
            _stack.Push(t);
        }

        protected T Pull()
        {
            if (_stack.Count == 0)
                _stack.Push(Instantiate(_prefab, transform).GetComponent<T>());
            
            return _stack.Pop();
        }
    }
}
