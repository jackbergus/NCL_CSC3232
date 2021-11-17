using System;

namespace ConsoleApp2.utils
{
    class UniformRandom {
        double a, b;

        public UniformRandom(double a = 0.0, double b = 1.0) {
            this.a = a;
            this.b = b;
        }

        public double nextReal(ref Random r) {
            return a + r.NextDouble() * (b - a);
        }

        public int nextInt(ref Random r) {
            return r.Next(((int)Math.Round(a)), ((int)Math.Round(b))+1);
        }
    }
}
