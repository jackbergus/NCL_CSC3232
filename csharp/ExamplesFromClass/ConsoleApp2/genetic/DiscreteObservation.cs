using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2.genetic
{
    public class DiscreteObservation<T>
    {
        Dictionary<T, int> _dictionary;
        private ArrayList objects;
        public DiscreteObservation()
        {
            _dictionary = new Dictionary<T, int>();
            objects = new ArrayList();
        }

        public T fromId(int x)  {
            return (T)objects[x];
        }
        
        public int size()  {
            return _dictionary.Count;
        }

        public bool Add(T obj) {
            if (_dictionary.ContainsKey(obj))
            {
                return false;
            }
            else
            {
                objects.Add(obj);
                _dictionary[obj] = _dictionary.Count;
                return true;
            }
        }

        public Int32 getId(T obj)
        {
            if (_dictionary.ContainsKey(obj))
                return _dictionary[obj];
            return -1;
        }

        public T[] ToArray()
        {
            return _dictionary.Keys.ToArray();
        }

        public override int GetHashCode()
        {
            int count = 0;
            foreach (var x in _dictionary.Keys)
                count *= x.GetHashCode();
            return count;
        }
    }
}
