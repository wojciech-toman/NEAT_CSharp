﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
	[Serializable]
	// A node (neuron) in the network. It can be marked as SENSOR (input), BIAS, OUTPUT, HIDDEN (belonging to one of the hidden layers).
	public class Node
	{
		public enum ENodeType
		{
			HIDDEN = 0,
			SENSOR = 1,
			OUTPUT = 2,
			BIAS = 3
		};

		private List<Link> incomingLinks = new List<Link>();
		private List<Link> outcomingLinks = new List<Link>();


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;

            Node n = (Node)obj;

            return (this.ID == n.ID && this.IsActive == n.IsActive && this.NodeType == n.NodeType &&
                this.incomingLinks.Count == n.incomingLinks.Count && this.outcomingLinks.Count == n.outcomingLinks.Count &&
                this.LastActivation == n.LastActivation && this.LastActivation2 == n.LastActivation2 && this.Activation == n.Activation &&
                this.ActivationCount == n.ActivationCount && this.ActivationOut == n.ActivationOut && this.ActivationSum == n.ActivationSum);
        }

        public void AddIncomingLink(Link lnk)
		{
			this.incomingLinks.Add(lnk);
		}

        public List<Link> IncomingLinks => this.incomingLinks;

        public List<Link> OutcomingLinks => this.outcomingLinks;

        public void AddOutcomingLink(Link lnk)
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
        public float ActivationOut => (this.ActivationCount > 0 ? this.Activation : 0.0f);
        public bool IsActive { get; set; }
		public float ActivationSum { get; set; }
		public float LastActivation { get; set; }
		public float LastActivation2 { get; set; }

		public Node Copy()
		{
			return new Node(this.NodeType, this.ID, this.Activation);
		}

		public void Reset()
		{
			// A sensor should not flush black
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
						lnk.InNode.Reset();
				}
			}
		}

		public int GetDepth(int depth)
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
					curDepth = lnk.InNode.GetDepth(depth + 1);
					if (curDepth > maxDepth) maxDepth = curDepth;
				}

				return maxDepth;
			}
		}
	}
}
