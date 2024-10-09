using System;
public class MyTime
{

    public MyTime() { }

    private static System.Diagnostics.Stopwatch watch = null;
    public long deltaTime
    {
        get
        {
            if (watch is null) { 
                watch = System.Diagnostics.Stopwatch.StartNew();
                return 0;
            }
            else
            {
                watch.Stop();
                var val = watch.ElapsedMilliseconds;
                watch = System.Diagnostics.Stopwatch.StartNew();
                return val;
            }
        }
    }
}