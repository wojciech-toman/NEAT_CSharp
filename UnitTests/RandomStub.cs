using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    class RandomStub : Random
    {
        public int NextDoubleValue { get; set; }
        public int NextIntValue { get; set; }

        public override double NextDouble()
        {
            return this.NextDoubleValue;
        }

        public override int Next(int maxValue)
        {
            return this.NextIntValue;
        }

        public override int Next()
        {
            return this.NextIntValue;
        }

        public override int Next(int minValue, int maxValue)
        {
            return this.NextIntValue;
        }
    }
}
