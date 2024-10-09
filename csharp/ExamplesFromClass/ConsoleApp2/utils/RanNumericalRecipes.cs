namespace ConsoleApp2.utils
{
    public class RanNumericalRecipes
    {
        private ulong x;

        RanNumericalRecipes(ulong x)  {
            this.x = x;
        }

        public ulong nextUlong()
        {
            var x = int64_(this.x);
            this.x++;
            return x;
        }
        
        public double nextDouble()
        {
            var x = doub(this.x);
            this.x++;
            return x;
        }
        
        public static ulong int64_(ulong jump) {
            ulong v = jump * 3935559000370003845L + 2691343689449507681L;
            v ^= v >> 21;
            v ^= v << 37;
            v ^= v >> 4;
            v *= 4768777513237032717L;
            v ^= v << 20;
            v ^= v >> 41;
            v ^= v << 5;
            return v;
        }

        public static double doub(ulong jump) {
            return 5.42101086242752217E-20 * (double)int64_(jump);
        }
        
        
    }
}