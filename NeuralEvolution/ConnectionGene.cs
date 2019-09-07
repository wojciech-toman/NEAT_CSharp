using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution
{
	// ConnectionGene is equivalent of directed edge in the graph/network. It connects 2 nodes with a given weight.
	// Additionally, it can be enabled or disabled.
	[Serializable]
	public class ConnectionGene
	{
		public Node InNodeGene { get; set; }
		public Node OutNodeGene { get; set; }
		public float Weight { get; set; }
		public bool IsEnabled { get; set; }
		public int Innovation { get; set; }
		public bool IsRecurrent { get; set; }

		public ConnectionGene(Node inNode, Node outNode, bool isRecurrent, float weight, bool isEnabled, int innovation)
		{
			this.InNodeGene = inNode;
			this.OutNodeGene = outNode;
			this.Weight = weight;
			this.IsEnabled = isEnabled;
			this.Innovation = innovation;
			this.IsRecurrent = isRecurrent;
		}


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;

            ConnectionGene rhs = (ConnectionGene)obj;

            return (this.InNodeGene.ID == rhs.InNodeGene.ID && this.OutNodeGene.ID == rhs.OutNodeGene.ID && this.Weight == rhs.Weight && 
                this.IsEnabled == rhs.IsEnabled && this.Innovation == rhs.Innovation && this.IsRecurrent == rhs.IsRecurrent);
        }

        public ConnectionGene copy()
		{
			return new ConnectionGene(this.InNodeGene.copy(), this.OutNodeGene.copy(), this.IsRecurrent, this.Weight, this.IsEnabled, this.Innovation);
		}
	}
}
