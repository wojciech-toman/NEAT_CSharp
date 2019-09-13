using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NEAT_CSharp
{
	[Serializable]
	// Genome represents a single Neural Network.
	// You need to create at least one object of Genome type and pass it to Simulation constructor.
	public class Genome
	{
		private List<ConnectionGene> connectionGenes = new List<ConnectionGene>();
		private List<Node> nodes = new List<Node>();
		private Random random = null;
		private const int maxTries = 20;
		[NonSerialized] private bool phenotypeChanged = false;
		[NonSerialized] private Network network = null;

		[NonSerialized] private Species species = null;

		// Population to which this species belongs
		[NonSerialized] private Simulation parentSimulation = null;


		private int localInnovationCounter = 0;


		public int NextInnovationNumber()
		{
			return ++this.localInnovationCounter;
		}

        public int GetInnovationNumber()
        {
            return this.localInnovationCounter;
        }


		public bool IsPopulationChampion { get; set; }



		public Genome(Random rnd)
		{
			random = rnd;
		}

		// Create genome from the existing network (for instance loaded from file)
		public Genome(Network net, Random rnd) : this(rnd)
		{
			if (net == null) throw new ArgumentNullException(nameof(net));

			// First add all nodes
			foreach (Node n in net.Nodes)
			{
				this.AddNode(n.copy());
			}

			// And now create connections based on the links
			foreach (Link lnk in net.Links)
				this.AddConnectionGene(lnk.InNode.ID, lnk.OutNode.ID, this.NextInnovationNumber(), true, lnk.Weight);
		}

		public Species Species { get { return this.species; } set { this.species = value; } }

		public Genome Copy()
		{
			if (!typeof(Genome).IsSerializable)
			{
				throw new Exception("The source object must be serializable");
			}

			if (Object.ReferenceEquals(this, null))
			{
				throw new Exception("The source object must not be null");
			}

			Genome result = default(Genome);

			using (var memoryStream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				memoryStream.Seek(0, SeekOrigin.Begin);
				result = (Genome)formatter.Deserialize(memoryStream);
			}

			result.random = this.random;

			return result;
		}

        private double fitness = 0.0;
		public double Fitness
		{
			get { return this.fitness; }
			set { this.fitness = value; }
		}
		public double OriginalFitness { get; set; }

		public float Error { get; set; }
		public bool ShouldBeEliminated { get; set; }

		public Simulation ParentSimulation { get { return this.parentSimulation; } set { this.parentSimulation = value; } }

		public Node GetNodeById(int id)
		{
			foreach (Node n in this.nodes)
				if (n.ID == id) return n;

			return null;
		}

		public Network GetNetwork()
		{
			if (this.phenotypeChanged || this.network == null)
			{
				// Network wasn't generated yet, or genome changed since last time,
                // so we need to recreate the network first
				this.network = new Network();
				foreach (Node n in this.nodes)
				{
					this.network.AddNode(n.copy());
				}

				foreach (ConnectionGene gene in this.connectionGenes)
				{
					if (gene.IsEnabled)
						this.network.AddLink(new Link(gene.InNodeGene, gene.OutNodeGene, gene.IsRecurrent, gene.Weight));
				}

				this.phenotypeChanged = false;
			}

			return this.network;
		}

		// Adds a new node to the genome
		public void AddNode(Node node)
		{
			this.nodes.Add(node);
			// Order nodes by ID
			this.nodes.Sort((x, y) => (x.ID.CompareTo(y.ID)));
			this.phenotypeChanged = true;
		}

		public void InsertConnectionGene(ConnectionGene gene)
		{
			if (gene == null) throw new ArgumentNullException(nameof(gene));

			int innovation = gene.Innovation;
            int i;
            for (i = 0; i < this.connectionGenes.Count; ++i)
			{
				if (this.connectionGenes[i].Innovation >= innovation)
					break;
			}
			this.connectionGenes.Insert(i, gene);
		}

		// Adds a connection gene to the genome. Connection gene is equivalent of an edge in the network (it connects two nodes)
		public void AddConnectionGene(ConnectionGene gene)
		{
			this.InsertConnectionGene(gene);

			// Add nodes if necessary
			bool foundNode1 = false, foundNode2 = false;
			foreach (Node n in this.nodes)
			{
				if (n.ID == gene.InNodeGene.ID) foundNode1 = true;
				if (n.ID == gene.OutNodeGene.ID) foundNode2 = true;

				if (foundNode1 && foundNode2) break;
			}

			if (!foundNode1) this.AddNode(gene.InNodeGene.copy());
			if (!foundNode2) this.AddNode(gene.OutNodeGene.copy());

			this.phenotypeChanged = true;
		}

		// Adds a connection gene to the genome by connecting nodes with inNode and outNode indices and setting the weight.
		// Connection gene is equivalent of an edge in the network (it connects two nodes)
		public void AddConnectionGene(int inNode, int outNode, float weight)
		{
			this.AddConnectionGene(inNode, outNode, this.NextInnovationNumber(), true, weight);
		}

		// Adds a connection gene to the genome. Connection gene is equivalent of an edge in the network (it connects two nodes)
		public void AddConnectionGene(int inNode, int outNode, bool isEnabled=true)
		{
			this.InsertConnectionGene(new ConnectionGene(this.GetNodeById(inNode), this.GetNodeById(outNode), false, 1.0f, isEnabled, this.NextInnovationNumber()));
			this.phenotypeChanged = true;
		}

		public void AddConnectionGene(int inNode, int outNode, int innovation, bool isEnabled=true, float weight=0.0f)
        {
			this.InsertConnectionGene(new ConnectionGene(this.GetNodeById(inNode), this.GetNodeById(outNode), false, weight, isEnabled, innovation));
			this.phenotypeChanged = true;
		}

        public List<ConnectionGene> ConnectionGenes => this.connectionGenes;

        // Helper method printing basic information about the genome
        public void DebugPrint()
		{
			for (int i = 0; i < this.connectionGenes.Count; ++i)
			{
				Console.WriteLine(string.Format("{0}: {1} -> {2} | weight: {3} | {4}", this.connectionGenes[i].Innovation,
					this.connectionGenes[i].InNodeGene.ID, this.connectionGenes[i].OutNodeGene.ID,
					this.connectionGenes[i].Weight, this.connectionGenes[i].IsEnabled ? "" : "DIS"));
			}
		}

		#region Mutations
		public Genome Crossover(Genome gen2, Random rnd)
        {
            if (gen2 == null) throw new ArgumentNullException(nameof(gen2));
            if (rnd == null) throw new ArgumentNullException(nameof(rnd));

            Genome gen1 = this;
            Genome child = new Genome(rnd);
            child.ParentSimulation = gen1.ParentSimulation;

            Genome moreFit, lessFit;
            GetMoreAndLessFit(gen2, gen1, out moreFit, out lessFit);

            // Make sure all inputs and outputs are included in the child genome
            foreach (Node n in gen1.Nodes)
            {
                if (n.NodeType == Node.ENodeType.SENSOR || n.NodeType == Node.ENodeType.BIAS || n.NodeType == Node.ENodeType.OUTPUT)
                    child.AddNode(n.copy());
            }

            foreach (ConnectionGene gene1 in moreFit.ConnectionGenes)
            {
                bool foundMatch = false;
                bool skip = false;
                ConnectionGene newGene = null;
                foreach (ConnectionGene gene2 in lessFit.ConnectionGenes)
                {
                    // Found matching gene, so add it to the child by randomly selecting from one of the parents
                    if (gene1.Innovation == gene2.Innovation)
                    {
                        foundMatch = true;
                        newGene = rnd.Next(2) == 0 ? gene1.copy() : gene2.copy();
                        // If one of the parent genes is disabled, there's big chance children's version will be too
                        if (!gene1.IsEnabled || !gene2.IsEnabled)
                            newGene.IsEnabled = rnd.NextDouble() > this.ParentSimulation.Parameters.DisableGeneProbability;
                        break;
                    }
                }

                // Disjoint or excess node
                if (!foundMatch)
                {
                    newGene = gene1;
                }

                // Check if there aren't any conflicts with an existing connection
                foreach (ConnectionGene g in child.ConnectionGenes)
                {
                    if ((g.InNodeGene.ID == newGene.InNodeGene.ID && g.OutNodeGene.ID == newGene.OutNodeGene.ID && g.IsRecurrent == newGene.IsRecurrent) ||
                        (g.InNodeGene.ID == newGene.OutNodeGene.ID && g.OutNodeGene.ID == newGene.InNodeGene.ID && !g.IsRecurrent && !newGene.IsRecurrent))
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                    child.AddConnectionGene(newGene);
            }


            return child;
        }

        // Performs crossover by averaging both genomes
        public Genome CrossoverAverage(Genome gen2, Random rnd)
		{
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));
			if (rnd == null) throw new ArgumentNullException(nameof(rnd));

			Genome gen1 = this;
			Genome child = new Genome(rnd);
			child.ParentSimulation = gen1.ParentSimulation;

			Genome moreFit, lessFit;
			GetMoreAndLessFit(gen2, gen1, out moreFit, out lessFit);

			// Make sure all inputs and outputs are included in the child genome
			foreach (Node n in gen1.Nodes)
			{
				if (n.NodeType == Node.ENodeType.SENSOR || n.NodeType == Node.ENodeType.BIAS || n.NodeType == Node.ENodeType.OUTPUT)
					child.AddNode(n.copy());
			}

			foreach (ConnectionGene gene1 in moreFit.ConnectionGenes)
			{
				bool foundMatch = false;
				bool skip = false;
				ConnectionGene newGene = null;
				foreach (ConnectionGene gene2 in lessFit.ConnectionGenes)
				{
					// Found matching gene, so add it to the child by randomly selecting from one of the parents
					if (gene1.Innovation == gene2.Innovation)
					{
						foundMatch = true;
						newGene = rnd.NextDouble() < 0.5 ? gene1.copy() : gene2.copy();
						newGene.Weight = (gene1.Weight + gene2.Weight) / 2.0f;
						newGene.IsRecurrent = rnd.NextDouble() < 0.5 ? gene1.IsRecurrent : gene2.IsRecurrent;
						newGene.Innovation = gene1.Innovation;
						// If one of the parent genes is disabled, there's big chance children's version will be too
						if (!gene1.IsEnabled || !gene2.IsEnabled)
							newGene.IsEnabled = rnd.NextDouble() > this.ParentSimulation.Parameters.DisableGeneProbability;
						break;
					}
				}

				// Disjoint or excess node
				if (!foundMatch)
					newGene = gene1;

				// Check if there aren't any conflicts with an existing connection
				foreach (ConnectionGene g in child.ConnectionGenes)
				{
					if ((g.InNodeGene.ID == newGene.InNodeGene.ID && g.OutNodeGene.ID == newGene.OutNodeGene.ID && g.IsRecurrent == newGene.IsRecurrent) ||
						(g.InNodeGene.ID == newGene.OutNodeGene.ID && g.OutNodeGene.ID == newGene.InNodeGene.ID && !g.IsRecurrent && !newGene.IsRecurrent))
					{
						skip = true;
						break;
					}
				}

				if (!skip)
					child.AddConnectionGene(newGene);
			}


			return child;
		}

        public static void GetMoreAndLessFit(Genome gen2, Genome gen1, out Genome moreFit, out Genome lessFit)
        {
            if (gen1.OriginalFitness >= gen2.OriginalFitness)
            {
                moreFit = gen1;
                lessFit = gen2;
            }
            else
            {
                moreFit = gen2;
                lessFit = gen1;
            }
        }

        // Mutation that creates new connection between 2 nodes.
        public void AddConnectionMutation(List<Innovation> innovations)
		{
			if (innovations == null) throw new ArgumentNullException(nameof(innovations));

			// Find first non-sensor node, so the target is never a sensor
			int minTargetNode = 0;
			while (this.nodes[minTargetNode].NodeType == Node.ENodeType.SENSOR || this.nodes[minTargetNode].NodeType == Node.ENodeType.BIAS)
			{
				minTargetNode++;
			}

			bool doRecurrency = this.random.NextDouble() < this.ParentSimulation.Parameters.RecurrencyProbability;
			bool recurFlag = false;
			bool found = false;
			Node node1 = null, node2 = null;

			if (doRecurrency)
			{
				int counter = 0;
				while (counter++ < maxTries)
				{
					bool recurrentLoop = this.random.NextDouble() > 0.5;

					int startNode = this.random.Next(0, this.nodes.Count);
					int targetNode = recurrentLoop ? startNode : this.random.Next(minTargetNode, this.nodes.Count);


					// Check if connection exists between the nodes
					int i = 0;
					node1 = this.nodes[startNode];
					node2 = this.nodes[targetNode];
					for (i = 0; i < this.connectionGenes.Count; ++i)
					{
						if ((node2.NodeType == Node.ENodeType.SENSOR || node2.NodeType == Node.ENodeType.BIAS) ||
							(connectionGenes[i].InNodeGene.ID == node1.ID && connectionGenes[i].OutNodeGene.ID == node2.ID && connectionGenes[i].IsRecurrent))
							break;
						//if (connectionGenes[i].InNodeGene.ID == node1.ID && connectionGenes[i].OutNodeGene.ID == node2.ID)
						//	break;
					}
					if (i == this.connectionGenes.Count)
					{
						Network net = this.GetNetwork();
						Node netNode1 = net.GetNodeById(node1.ID);
						Node netNode2 = net.GetNodeById(node2.ID);
						if (netNode1 != null && netNode2 != null)
							recurFlag = (parentSimulation.Parameters.RecurrencyProbability > 0.0f && net.IsRecurrentConnection(netNode1, netNode2, 0, this.nodes.Count * this.nodes.Count));
						// Connections outgoing from Output nodes are considered recurrent
						if (node1.NodeType == Node.ENodeType.OUTPUT)
							recurFlag = true;

						if (!recurFlag)
							continue;
						else
							found = true;

						break;
					}
				}
			}
			else
			{
				int counter = 0;
				while (counter++ < maxTries)
				{
					int startNode = this.random.Next(0, this.nodes.Count);
					int targetNode = this.random.Next(minTargetNode, this.nodes.Count);

					if (startNode != targetNode)
					{
						// Check if connection exists between the nodes
						int i = 0;
						node1 = this.nodes[startNode];
						// Don't allow recurrencies for now
						if (node1.NodeType == Node.ENodeType.OUTPUT)
							continue;
						node2 = this.nodes[targetNode];
						for (i = 0; i < this.connectionGenes.Count; ++i)
						{
							if (connectionGenes[i].InNodeGene.ID == node1.ID && connectionGenes[i].OutNodeGene.ID == node2.ID && !connectionGenes[i].IsRecurrent)
								break;
						}
						if (i == this.connectionGenes.Count)
						{
							Network net = this.GetNetwork();
							Node netNode1 = net.GetNodeById(node1.ID);
							Node netNode2 = net.GetNodeById(node2.ID);
							if(netNode1 != null && netNode2 != null)
								recurFlag = (parentSimulation.Parameters.RecurrencyProbability > 0.0f && net.IsRecurrentConnection(netNode1, netNode2, 0, this.nodes.Count * this.nodes.Count));
							// Connections outgoing from Output nodes are considered recurrent
							if (node1.NodeType == Node.ENodeType.OUTPUT)
								recurFlag = true;

							if (recurFlag)
								continue;
							else
								found = true;

							break;
						}
					}
				}
			}

			// Check if this is new innovation, or whether it already exists somewhere in the population
			if (found)
			{
				if (doRecurrency) recurFlag = true;

				bool innovationFound = false;
				ConnectionGene newGene = null;
				foreach (Innovation innov in innovations)
				{
					if (innov.InnovationType == Innovation.EInnovationType.NEWLINK && innov.InNode.ID == node1.ID && innov.OutNode.ID == node2.ID)
					{
						innovationFound = true;
						newGene = new ConnectionGene(innov.InNode, innov.OutNode, recurFlag, innov.Weight, true, innov.ID);
						break;
					}
				}

				// This is completely new innovation
				if (!innovationFound)
				{
					float newRandomWeight = (float)(this.random.NextDouble() * 2.0f - 1.0f);
					//this.connectionGenes.Add(new ConnectionGene(this.getNode(startNode), this.getNode(targetNode), newRandomWeight, true, this.NextInnovationNumber()));
					//this.addConnection(new ConnectionGene(this.getNode(startNode), this.getNode(targetNode), newRandomWeight, true, this.NextInnovationNumber()));
					Innovation newInnov = new Innovation(Innovation.EInnovationType.NEWLINK, node1, node2, null, Innovation.GetNextID(), 0, newRandomWeight, 0);
					innovations.Add(newInnov);

					newGene = new ConnectionGene(node1, node2, recurFlag, newRandomWeight, true, newInnov.ID);
				}

				this.AddConnectionGene(newGene);

				this.phenotypeChanged = true;
			}
		}

		// Mutation that creates new node in the genome.
		public void AddNodeMutation(List<Innovation> innovations)
        {
            if (innovations == null) throw new ArgumentNullException(nameof(innovations));

            // If there are no genes at all, return
            if (connectionGenes.Count == 0)
                return;

            bool found = false;
            int connectionIndex = -1;
            found = this.FindConnectionGeneToSplit(out connectionIndex);

            // We haven't found any proper gene to split, so exit
            if (!found)
                return;

            ConnectionGene oldConnection = this.connectionGenes[connectionIndex];
            oldConnection.IsEnabled = false;

            // Check if this is new innovation, or whether it already exists somewhere in the population
            bool innovationFound = false;
            Node newNode = null;
            ConnectionGene newGene1 = null;
            ConnectionGene newGene2 = null;
            foreach (Innovation innov in innovations)
            {
                if (innov.InnovationType == Innovation.EInnovationType.NEWNODE &&
                    innov.InNode.ID == oldConnection.InNodeGene.ID &&
                    innov.OutNode.ID == oldConnection.OutNodeGene.ID &&
                    oldConnection.Innovation == innov.OldID)
                {
                    innovationFound = true;

                    newNode = new Node(Node.ENodeType.HIDDEN, innov.NewNode.ID);

                    newGene1 = new ConnectionGene(oldConnection.InNodeGene, newNode, oldConnection.IsRecurrent, 1.0f, true, innov.ID);
                    newGene2 = new ConnectionGene(newNode, oldConnection.OutNodeGene, false, oldConnection.Weight, true, innov.ID2);

                    break;
                }
            }

            // Create new hidden node between old nodes and link it with the network
            if (!innovationFound)
            {
                newNode = new Node(Node.ENodeType.HIDDEN, this.nodes.Count + 1);
                int id1 = Innovation.GetNextID();
                int id2 = Innovation.GetNextID();

                Innovation newInnov = new Innovation(Innovation.EInnovationType.NEWNODE, oldConnection.InNodeGene, oldConnection.OutNodeGene, newNode, id1, id2, 0.0f, oldConnection.Innovation);
                innovations.Add(newInnov);

                newGene1 = new ConnectionGene(oldConnection.InNodeGene, newNode, oldConnection.IsRecurrent, 1.0f, true, id1);
                newGene2 = new ConnectionGene(newNode, oldConnection.OutNodeGene, false, oldConnection.Weight, true, id2);
            }

            this.AddNode(newNode);
            this.AddConnectionGene(newGene1);
            this.AddConnectionGene(newGene2);

            this.phenotypeChanged = true;
        }

        public bool FindConnectionGeneToSplit(out int connectionIndex)
        {
            bool found = false;
            connectionIndex = -1;

            if (connectionGenes.Count == 0)
                return false;

            int counter = 0;
            while (!found && counter++ < 20)
            {
                int candidateIndex = this.random.Next(connectionGenes.Count);
                if (this.connectionGenes[candidateIndex].IsEnabled)
                {
                    connectionIndex = candidateIndex;
                    found = true;
                    break;
                }
            }

            return found;
        }

        // Mutation that changes connection weights.
        public void MutateWeights(float mutationPower)
		{
			bool severeMutation = false;
			if (this.random.NextDouble() > 0.5) severeMutation = true;
			else severeMutation = false;

			double rate = 1.0;
			double gausspoint, coldgausspoint;
			double genesCount = this.connectionGenes.Count;
			double endpart = genesCount * 0.8;
			double counter = 0.0;

			foreach (ConnectionGene gene in this.connectionGenes)
			{
				if (severeMutation)
				{
					gausspoint = 0.3;
					coldgausspoint = 0.1;
				}
				else if ((genesCount >= 10.0) && (counter > endpart))
				{
					gausspoint = 0.5;       // Mutate by modification % of connections
					coldgausspoint = 0.3;   // Mutate the rest by replacement % of the time
				}
				else
				{
					// Half the time don't do any cold mutations
					if (this.random.NextDouble() > 0.5)
					{
						gausspoint = 1.0 - rate;
						coldgausspoint = 1.0 - rate - 0.1;
					}
					else
					{
						gausspoint = 1.0 - rate;
						coldgausspoint = 1.0 - rate;
					}
				}

				double randomWeight = (this.random.NextDouble() * 2.0f - 1.0f) * mutationPower;

				double randchoice = this.random.NextDouble();
				if (randchoice > gausspoint)
					gene.Weight += (float)randomWeight;
				else if (randchoice > coldgausspoint)
					gene.Weight = (float)randomWeight;


				if (this.ParentSimulation.Parameters.AreConnectionWeightsCapped)
				{
					if (gene.Weight > this.ParentSimulation.Parameters.MaxWeight) gene.Weight = this.ParentSimulation.Parameters.MaxWeight;
					if (gene.Weight < -this.ParentSimulation.Parameters.MaxWeight) gene.Weight = -this.ParentSimulation.Parameters.MaxWeight;
				}

				counter += 1.0;
			}

			this.phenotypeChanged = true;
		}

		// Mutation that reenables previously disabled connection gene
		public void ReenableMutation()
		{
			// Find first disabled gene and reenable it
			foreach (ConnectionGene gene in this.connectionGenes)
			{
				if (!gene.IsEnabled)
				{
					gene.IsEnabled = true;
					break;
				}
			}
			this.phenotypeChanged = true;
		}

		#endregion

		public static int ExcessGenesCount(Genome gen1, Genome gen2)
		{
			if (gen1 == null) throw new ArgumentNullException(nameof(gen1));
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));

			// Handle special case when one of the genomes is completely empty
			if (gen1.ConnectionGenes.Count == 0 || gen2.ConnectionGenes.Count == 0)
				return Math.Max(gen1.ConnectionGenes.Count, gen2.ConnectionGenes.Count);

			int excessCount = 0;

			List<ConnectionGene> connGenes1 = gen1.ConnectionGenes;
			List<ConnectionGene> connGenes2 = gen2.ConnectionGenes;

			int i = 0, j = 0;
			while (i < connGenes1.Count || j < connGenes2.Count)
			{
				if (i == connGenes1.Count) { excessCount++; ++j; }
				else if (j == connGenes2.Count) { excessCount++; ++i; }
				else
				{
					int innovation1 = connGenes1[i].Innovation;
					int innovation2 = connGenes2[j].Innovation;

					if (innovation1 == innovation2) { ++i; ++j; }
					else if (innovation1 < innovation2) { ++i; }
					else if (innovation1 > innovation2) { ++j; }
				}
			}

			return excessCount;
		}

		public static int DisjointGenesCount(Genome gen1, Genome gen2)
		{
			if (gen1 == null) throw new ArgumentNullException(nameof(gen1));
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));

			// Handle special case when one of the genomes is completely empty
			if (gen1.ConnectionGenes.Count == 0 || gen2.ConnectionGenes.Count == 0)
				return 0;

			int disjointCount = 0;

			List<ConnectionGene> connGenes1 = gen1.ConnectionGenes;
			List<ConnectionGene> connGenes2 = gen2.ConnectionGenes;

			int i = 0, j = 0;
			while (i < connGenes1.Count && j < connGenes2.Count)
			{
				int innovation1 = connGenes1[i].Innovation;
				int innovation2 = connGenes2[j].Innovation;

				if (innovation1 == innovation2) { ++i; ++j; }
				else if (innovation1 < innovation2) { disjointCount++; ++i; }
				else if (innovation1 > innovation2) { disjointCount++; ++j; }
			}

			return disjointCount;
		}

		public static int MatchingGenesCount(Genome gen1, Genome gen2)
		{
			if (gen1 == null) throw new ArgumentNullException(nameof(gen1));
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));

			// Handle special case when one of the genomes is completely empty
			if (gen1.ConnectionGenes.Count == 0 || gen2.ConnectionGenes.Count == 0)
				return 0;

			int matchingCount = 0;

			List<ConnectionGene> connGenes1 = gen1.ConnectionGenes;
			List<ConnectionGene> connGenes2 = gen2.ConnectionGenes;

			int i = 0, j = 0;
			while (i < connGenes1.Count && j < connGenes2.Count)
			{
				int innovation1 = connGenes1[i].Innovation;
				int innovation2 = connGenes2[j].Innovation;

				if (innovation1 == innovation2) { matchingCount++; ++i; ++j; }
				else if (innovation1 < innovation2) { ++i; }
				else if (innovation1 > innovation2) { ++j; }
			}

			return matchingCount;
		}

		public static float GetAverageWeightDifference(Genome gen1, Genome gen2)
		{
			if (gen1 == null) throw new ArgumentNullException(nameof(gen1));
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));

			// Handle special case when one of the genomes is completely empty
			if (gen1.ConnectionGenes.Count == 0 || gen2.ConnectionGenes.Count == 0)
				return 0.0f;

			int matchingCount = Genome.MatchingGenesCount(gen1, gen2);

			float difference = 0.0f;

			List<ConnectionGene> connGenes1 = gen1.ConnectionGenes;
			List<ConnectionGene> connGenes2 = gen2.ConnectionGenes;

			int i = 0, j = 0;
			while (i < connGenes1.Count && j < connGenes2.Count)
			{
				int innovation1 = connGenes1[i].Innovation;
				int innovation2 = connGenes2[j].Innovation;

				if (innovation1 == innovation2)
				{
					difference += (float)Math.Abs(connGenes1[i].Weight - connGenes2[j].Weight);
					++i; ++j;
				}
				else if (innovation1 < innovation2) { ++i; }
				else if (innovation1 > innovation2) { ++j; }
			}

			return difference / matchingCount;
		}

		// Calculates compatibility distance between 'this' genome and another one. Result is based on the difference in the
		// number of disjoint and excess nodes as well as on an average connection weight difference
		// TODO: compatibility distance should calculate everything in one place to avoid too many 'while' loops for
		// performance reasons
		public float CompatibilityDistance(Genome gen2)
		{
			if (gen2 == null) throw new ArgumentNullException(nameof(gen2));

			int N = Math.Max(this.ConnectionGenes.Count, gen2.ConnectionGenes.Count);
			// Handle special case when both genomes have no genes at all
			if (N == 0) return 0.0f;

			// TODO: if N >= 20 than c1, c2 and c3 parameters might need adjustments
			N = 1;
			//if (N < 20) N = 1;

			float avgWeightDiff = Genome.GetAverageWeightDifference(this, gen2);

			float c1 = this.ParentSimulation.Parameters.ExcessGenessCoeff;
			float c2 = this.ParentSimulation.Parameters.DisjointGenesCoeff;
			float c3 = this.ParentSimulation.Parameters.WeightDiffCoeff;

			return (c1 * Genome.ExcessGenesCount(this, gen2) / N) + (c2 * Genome.DisjointGenesCount(this, gen2) / N) + (c3 * avgWeightDiff);
		}

        public List<Node> Nodes => this.nodes;

        public void ToggleEnabledMutation()
		{
			// Handle special case when one of the genomes is completely empty
			if (this.connectionGenes.Count == 0)
				return;

			int counter = 0;
			bool isOkToToggle = false;
			ConnectionGene gene = null;
			while (counter++ < maxTries && !isOkToToggle)
			{
				gene = this.connectionGenes[this.random.Next(connectionGenes.Count)];
				// If gene is enabled, we have first check if that's safe to disable it:
				// i.e. if there exists another gene connecting to the In-Node.
				if (gene.IsEnabled)
				{
					foreach (ConnectionGene checkGene in this.connectionGenes)
					{
						if (checkGene.InNodeGene == gene.InNodeGene && checkGene.IsEnabled && checkGene.Innovation != gene.Innovation)
						{
							isOkToToggle = true;
							break;
						}
					}
				}
				else
				{
					// It's always safe to reenable the gene
					isOkToToggle = true;
				}
			}

			// Toggle gene's enabled state if it's safe to do so
			if (isOkToToggle)
			{
				gene.IsEnabled = !gene.IsEnabled;
				this.phenotypeChanged = true;
			}
		}
	}
}
