using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using NEAT_CSharp.ActivationFunctions;

namespace NEAT_CSharp
{
	[Serializable]
	// Neural network with nodes and connections between them.
	public class Network
	{
		private List<Node> nodes = new List<Node>();
		private List<Node> inputNodes = new List<Node>();
		private List<Node> outputNodes = new List<Node>();

		private List<Link> links = new List<Link>();

        public ActivationFunction ActivationFunction { get; set; } = new SteepenedSigmoid();


        public void SerializeBinary(string fileName)
		{
			try
			{
				using (var fileStream = File.Create(fileName))
				{
					var formatter = new BinaryFormatter();
					formatter.Serialize(fileStream, this);
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}
		}

		public static Network DeserializeNetwork(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return null;

			Network result = new Network();
			try
			{
				using (var fileStream = File.OpenRead(fileName))
				{
					var formatter = new BinaryFormatter();
					result = (Network)formatter.Deserialize(fileStream);
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}

			return result;
		}


		public void AddNode(Node node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));

			this.nodes.Add(node);
			if (node.NodeType == Node.ENodeType.SENSOR || node.NodeType == Node.ENodeType.BIAS)
				this.inputNodes.Add(node);
			else if (node.NodeType == Node.ENodeType.OUTPUT)
				this.outputNodes.Add(node);
		}

        public void AddNodes(IEnumerable<Node> nodesCollection)
        {
            if(nodesCollection == null) throw new ArgumentNullException(nameof(nodesCollection));

            foreach(Node node in nodesCollection)
            {
                this.AddNode(node);
            }
        }

		// Call after adding all nodes
		public void AddLink(Link link)
		{
			if (link == null) throw new ArgumentNullException(nameof(link));

			this.links.Add(link);
			bool inNodeFound = false, outNodeFound = false;
			foreach (Node n in this.nodes)
			{
				if (n.ID == link.InNode.ID)
				{
					link.InNode = n;
					n.addOutcomingLink(link);

					inNodeFound = true;
				}

				if (n.ID == link.OutNode.ID)
				{
					link.OutNode = n;
					n.addIncomingLink(link);

					outNodeFound = true;
				}

				if (inNodeFound && outNodeFound)
                    break;
			}
		}

		public void SetInput(float[] input)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			if (input.Length != this.inputNodes.Count)
			    throw new ArgumentException($"Invalid number of input elements. Expected {inputNodes.Count}, was {input.Length}");

			// Set active state on the sensor nodes
			for (int i = 0; i < input.Length; ++i)
			{
				this.inputNodes[i].Activation = input[i];
				this.inputNodes[i].ActivationCount++;
			}
		}

		public List<Node> Output { get { return this.outputNodes; } }

		public List<Node> Input { get { return this.inputNodes; } }

		public List<Node> Nodes
		{
			get { return this.nodes; }
			set { this.nodes = value; }
		}

		public Node GetNodeById(int id)
		{
			foreach (Node n in this.nodes)
				if (n.ID == id) return n;

			return null;
		}

        public List<Link> Links
		{
			get { return this.links; }
			set { this.links = value; }
		}

        public bool Activate()
		{
			bool onetime = false;
			int counter = 0;
			const int maxTries = 20;

			while (this.IsOutputNotActivated() || !onetime)
            {
                // Too many failures - some outputs remain unactivated; exit
                if (counter++ > maxTries)
                    return false;

                // For each non-sensor node compute incoming activation
                this.ComputeNodesActivationSum();

                // Compute value of activation function (at the moment sigmoid)
                this.ComputeNodesActivationFunctionValue();

                onetime = true;
            }

            return true;
		}

        public void ComputeNodesActivationSum()
        {
            foreach (Node node in this.nodes)
            {
                if (node.NodeType == Node.ENodeType.SENSOR || node.NodeType == Node.ENodeType.BIAS)
                    continue;

                node.ActivationSum = 0.0f;
                node.IsActive = false;
                foreach (Link lnk in node.IncomingLinks)
                {
                    float toAdd = lnk.Weight * lnk.InNode.ActivationOut;
                    if (lnk.InNode.NodeType == Node.ENodeType.SENSOR || lnk.InNode.NodeType == Node.ENodeType.BIAS || lnk.InNode.IsActive)
                        node.IsActive = true;
                    node.ActivationSum += toAdd;
                }
            }
        }

        public void ComputeNodesActivationFunctionValue()
        {
            foreach (Node node in this.nodes)
            {
                if (node.NodeType == Node.ENodeType.SENSOR || node.NodeType == Node.ENodeType.BIAS)
                    continue;

                if (node.IsActive)
                {
                    node.LastActivation2 = node.LastActivation;
                    node.LastActivation = node.Activation;

                    node.Activation = ActivationFunction.CalculateActivation(node.ActivationSum);

                    node.ActivationCount++;
                }
            }
        }

        private bool IsOutputNotActivated()
		{
			foreach (Node output in this.outputNodes)
			{
				if (output.ActivationCount == 0) return true;
			}

			return false;
		}

		public void Reset()
		{
			foreach (Node n in this.nodes)
				n.reset();
		}

		public int GetMaxDepth()
		{
			int maxDepth = 0;
			foreach (Node n in this.outputNodes)
			{
				int depth = n.getDepth(0);
				if (depth > maxDepth) maxDepth = depth;
			}

			return maxDepth;
		}

		public bool IsRecurrentConnection(Node inNode, Node outNode, int count, int thresh)
		{
			if (++count > thresh)
				return false;

			if (inNode == null) throw new ArgumentNullException(nameof(inNode));

			if (inNode == outNode)
                return true;
			else
			{
				foreach(Link lnk in inNode.IncomingLinks)
				{
					if(!lnk.IsRecurrent)
						if (this.IsRecurrentConnection(lnk.InNode, outNode, count, thresh)) return true;
				}
			}

			return false;
		}
	}
}
