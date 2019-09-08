using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution.ActivationFunctions
{
    [Serializable]
    public class ReLU : ActivationFunction
    {
        public override float CalculateActivation(float input)
        {
            return input < 0.0f ? 0.0f : input;
        }
    }
}
