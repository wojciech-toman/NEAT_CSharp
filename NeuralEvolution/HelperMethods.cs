using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
    public static class HelperMethods
    {
        public static T Clamp<T>(this T value, T inclusiveMin, T inclusiveMax) where T : IComparable<T> {
            if (value.CompareTo(inclusiveMin) < 0) return inclusiveMin;
            if (value.CompareTo(inclusiveMax) > 0) return inclusiveMax;

            return value;
        }
    }
}
