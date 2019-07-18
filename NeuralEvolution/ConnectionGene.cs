using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution
{
	[Serializable]
	public class ConnectionGene
	{
		public Node InNodeGene { get; set; }
		public Node OutNodeGene { get; set; }
		public float Weight { get; set; }
		public bool IsEnabled { get; set; }
		public int Innovation { get; set; }
		public bool IsRecurrent { get; set; }

		public ConnectionGene(Node inNode, Node outNode, bool isRecurrent, float weight, bool isExpressed, int innovation)
		{
			this.InNodeGene = inNode;
			this.OutNodeGene = outNode;
			this.Weight = weight;
			this.IsEnabled = isExpressed;
			this.Innovation = innovation;
			this.IsRecurrent = isRecurrent;
		}

		public ConnectionGene copy()
		{
			return new ConnectionGene(this.InNodeGene.copy(), this.OutNodeGene.copy(), this.IsRecurrent, this.Weight, this.IsEnabled, this.Innovation);
		}
	}
}
