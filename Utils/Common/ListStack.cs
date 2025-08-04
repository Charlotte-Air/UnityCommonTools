using System;
using System.Collections.Generic;

namespace Framework.Utils
{
    public class ListStack<T> : List<T>
    {
        public T Peek() => this.Peek(0);

        public T Peek(int depth)
        {
            T obj;
            if (!this.TryPeek(depth, out obj))
                throw new InvalidOperationException();
            return obj;
        }

        public bool TryPeek(out T item) => this.TryPeek(0, out item);

        public bool TryPeek(int depth, out T item)
        {
            if (depth >= this.Count)
            {
                item = default(T);
                return false;
            }

            item = this[this.Count - depth - 1];
            return true;
        }

        public T Pop()
        {
            T obj;
            if (!this.TryPop(out obj))
                throw new InvalidOperationException();
            return obj;
        }

        public bool TryPop(out T item)
        {
            if (this.Count == 0)
            {
                item = default(T);
                return false;
            }

            item = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return true;
        }

        public void Push(T item) => this.Add(item);
    }
}