﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.ActivationFunctions
{
    [Serializable]
    public abstract class ActivationFunction
    {
        public abstract float CalculateActivation(float input);
    }
}
