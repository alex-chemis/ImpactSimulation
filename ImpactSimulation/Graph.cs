using System;

namespace ImpactSimulation
{
    static class Graph
    {
        public static void EllGraphSp(ref (double Y, double X) k, (double X, double Y) speed, (double X, double Y) mass)
        {
            k.Y = Math.Sqrt(Math.Pow(speed.X, 2) + Math.Pow(speed.Y, 2));
            k.X = k.Y;
            k.Y *= Math.Sqrt(mass.X);
            k.X *= Math.Sqrt(mass.Y);
        }

        public static void EllGraphPo(ref (double Y, double X) k, (double X, double Y) mass)
        {
            k.Y = Math.Sqrt(mass.X);
            k.X = Math.Sqrt(mass.Y);
        }

        public static void EllGraphCi(ref double k, (double X, double Y) speed, (double X, double Y) mass)
        {
            k = Math.Sqrt(mass.X * Math.Pow(speed.X, 2) + mass.Y * Math.Pow(speed.Y, 2));
        }
    }
}
