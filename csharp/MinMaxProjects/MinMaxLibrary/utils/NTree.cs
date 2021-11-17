using System;
using System.Collections.Generic;

namespace MinMaxLibrary.utils
{
    public class NTree<T>
    {
        public T data { get;  }
        private List<NTree<T>> children;

        public NTree(T data) {
            this.data = data;
            children = new List<NTree<T>>();
        }

        public NTree<T> AddChild(T data) {
            NTree<T> child = new NTree<T>(data);
            children.Add(child);
            return child;
        }

        public int getChildrenSize() {
            return children.Count;
        }

        public NTree<T> GetChild(int i) {
            return children[i];
        }

        public void Print(Func<T, string> printer, int depth = 0)
        {
            Console.WriteLine(new String('.', depth) + printer(this.data));
            foreach (NTree<T> child in children)   child.Print(printer, depth + 1); 
        }

    }

}
