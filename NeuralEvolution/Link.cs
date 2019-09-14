using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
	[Serializable]
	// An undirected edge in the network connecting two nodes
	public class Link
	{
		public Node InNode { get; set; }
		public Node OutNode { get; set; }
		public float Weight { get; set; }
		public bool IsRecurrent { get; set; }

		public Link()
		{

		}

		public Link(Node inNode, Node outNode, bool isRecurrent=false, float weight=0.0f)
		{
			this.InNode = inNode;
			this.OutNode = outNode;
			this.Weight = weight;
			this.IsRecurrent = isRecurrent;
		}
	}
}
