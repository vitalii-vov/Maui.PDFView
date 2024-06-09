namespace Example.Business.Collections
{
    public class LoopedList<T>
    {
        private readonly LinkedList<T> _items = new();

        public void Add(T value)
        {
            _items.AddLast(value);
        }

        public T Next()
        {
            var next = _items.First.Value;
            _items.RemoveFirst();
            _items.AddLast(next);
            return next;
        }
    }
}

