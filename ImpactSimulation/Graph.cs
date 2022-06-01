using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpactSimulation
{
    static class Graph
    {
        public static void EllGraphTr(ref (double Y, double X) k, (double X, double Y) speed, (double X, double Y) mass)
        {
            k.X = Math.Sqrt(Math.Pow(speed.X, 2) + Math.Pow(speed.Y, 2));
            k.Y = k.X;
            k.X *= Math.Sqrt(mass.X);
            k.Y *= Math.Sqrt(mass.Y);
        }
    }
}
