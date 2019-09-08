using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.ActivationFunctions
{
    [Serializable]
    public sealed class SteepenedSigmoid : ActivationFunction
    {
        public override float CalculateActivation(float input)
        {
            float constant = 4.924273f; // Used to steepen the function
            return (float)(1.0f / (1.0f + Math.Exp(-constant * input)));
        }
    }
}
