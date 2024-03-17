using System.Collections.Generic;

public class Subscription<T>
{
    private HashSet<T> _subscribers = new();

    public void Subscribe(T subscriber)
    {
        _subscribers.Add(subscriber);
    }

    public void Unsubscribe(T subscriber)
    {
        _subscribers.Remove(subscriber);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _subscribers.GetEnumerator();
    }
}