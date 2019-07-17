using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralEvolution
{
	[Serializable]
	public class Node
	{
		public enum ENodeType
		{
			HIDDEN = 0,
			SENSOR = 1,
			OUTPUT = 2,
			BIAS = 3
		};

		//static int nodeId;
		private List<Link> incomingLinks = new List<Link>();
		private List<Link> outcomingLinks = new List<Link>();

		public void addIncomingLink(Link lnk)
		{
			this.incomingLinks.Add(lnk);
		}

		public List<Link> getIncomingLinks()
		{
			return this.incomingLinks;
		}

		public List<Link> getOutcomingLinks()
		{
			return this.outcomingLinks;
		}

		public void addOutcomingLink(Link lnk)
		{
			this.outcomingLinks.Add(lnk);
		}

		/*public Node(ENodeType nodeType)
		{
			this.NodeType = nodeType;
			this.ID = Node.nodeId++;
            this.Activation = 1.0f;
		}*/

		public Node() : this(ENodeType.SENSOR, 0)
		{ }

		public Node(ENodeType nodeType, int id)
		{
			this.NodeType = nodeType;
			this.ID = id;
			this.Activation = 1.0f;
		}

		public Node(ENodeType nodeType, int id, float activation)
		{
			this.NodeType = nodeType;
			this.ID = id;
			this.Activation = activation;
		}

		public ENodeType NodeType { get; set; }
		public int ID { get; set; }
		public int ActivationCount { get; set; }
		public float Activation { get; set; }
		public float ActivationOut
		{
			get { return (this.ActivationCount > 0 ? this.Activation : 0.0f); }
		}
		public bool IsActive { get; set; }
		public float ActivationSum { get; set; }
		public float LastActivation { get; set; }
		public float LastActivation2 { get; set; }

		public Node copy()
		{
			return new Node(this.NodeType, this.ID, this.Activation);
		}

		public void reset()
		{
			//A sensor should not flush black
			if (this.NodeType == ENodeType.SENSOR || this.NodeType == ENodeType.BIAS)
			{
				this.ActivationCount = 0;
				this.Activation = 0;
				this.LastActivation = 0;
				this.LastActivation2 = 0;
			}
			else
			{
				if (this.ActivationCount > 0)
				{
					this.ActivationCount = 0;
					this.Activation = 0;
					this.LastActivation = 0;
					this.LastActivation2 = 0;
				}

				// Reset recursively
				foreach (Link lnk in this.incomingLinks)
				{
					//lnk.addedWeight = 0.0f;
					if (lnk.InNode.ActivationCount > 0)
						lnk.InNode.reset();
				}
			}
		}

		public int getDepth(int depth)
		{
			if (depth > 10)
				return 10;

			int maxDepth = depth;
			int curDepth = 0;

			if (this.NodeType == ENodeType.SENSOR || this.NodeType == ENodeType.BIAS)
				return depth;
			else
			{
				foreach (Link lnk in this.incomingLinks)
				{
					curDepth = lnk.InNode.getDepth(depth + 1);
					if (curDepth > maxDepth) maxDepth = curDepth;
				}

				return maxDepth;
			}
		}
	}
}
