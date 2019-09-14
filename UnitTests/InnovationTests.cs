using Microsoft.VisualStudio.TestTools.UnitTesting;
using NEAT_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.Tests
{
    [TestClass()]
    public class InnovationTests
    {
        [TestMethod()]
        public void GetNextIDTest()
        {
            Innovation.SetCurrentID(0);

            Assert.AreEqual(1, Innovation.GetNextID());
            Assert.AreEqual(2, Innovation.GetNextID());
        }
    }
}