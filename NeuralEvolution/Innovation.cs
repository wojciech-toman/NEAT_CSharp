using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
	public class Innovation
	{
		static int idCounter = 0;

		public static void SetCurrentID(int value)
		{
			idCounter = value;
		}

		public static int GetNextID()
		{
			return ++idCounter;
		}

		public enum EInnovationType
		{
			NEWLINK = 0,
			NEWNODE = 1,
		};

		public Innovation(EInnovationType type, Node inNode, Node outNode, Node newNode, int id, int id2, float weight, int oldID)
		{
			this.InnovationType = type;
			this.InNode = inNode;
			this.OutNode = outNode;
			this.ID = id;
			if (type == EInnovationType.NEWNODE)
			{
				this.NewNode = newNode;
				this.ID2 = id2;
				this.OldID = oldID;
			}
			else if (type == EInnovationType.NEWLINK)
			{
				this.Weight = weight;
			}
		}

		public EInnovationType InnovationType { get; set; }
		public Node InNode { get; set; }
		public Node OutNode { get; set; }
		public Node NewNode { get; set; }
		public float Weight { get; set; }
		public int ID { get; set; }
		public int ID2 { get; set; }
		public int OldID { get; set; }
	}
}
