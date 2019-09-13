using Microsoft.VisualStudio.TestTools.UnitTesting;
using NEAT_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.Tests
{
    [TestClass]
    public class SimulationTests
    {
        Random r = null;
        Genome gen = null;


        [TestInitialize]
        public void SetupTest()
        {
            r = new Random(0);

            // Genome 
            gen = new Genome(r);

            // Create 3 sensors
            gen.addNode(new Node(Node.ENodeType.SENSOR, 1));
            gen.addNode(new Node(Node.ENodeType.SENSOR, 2));
            gen.addNode(new Node(Node.ENodeType.SENSOR, 3));

            // Create 1 output
            gen.addNode(new Node(Node.ENodeType.OUTPUT, 4));

            // Create 1 hidden node
            gen.addNode(new Node(Node.ENodeType.HIDDEN, 5));

            // Add connections from the paper
            gen.addConnection(1, 4, 0.5f);
            gen.addConnection(2, 4, false);
            gen.addConnection(3, 4);
            gen.addConnection(2, 5);
            gen.addConnection(5, 4);
            gen.addConnection(1, 5, true, 8);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            r = null;
            gen = null;
        }

        [TestMethod]
        public void SimulationCreationTest_Valid()
        {
            Simulation sim = new Simulation(r, gen, 100);

            Assert.IsTrue(sim.Genomes.Count == 100);
            Assert.IsTrue(sim.EpochID == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimulationCreationTest_Invalid_NullGenome()
        {
            Simulation sim = new Simulation(r, null, 100);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [ExpectedException(typeof(ArgumentException))]
        public void SimulationCreationTest_Invalid_WrongPopulation(int populationSize)
        {
            Simulation sim = new Simulation(r, gen, populationSize);
        }

        [DataTestMethod]
        [DataRow(3, 2, 1)]
        [DataRow(3, 3, 3)]
        public void OrderSpeciesTest_SameOrder(int fitness1, int fitness2, int fitness3)
        {
            Simulation sim = new Simulation(r, gen, 1);

            // Create some fake species and genomes
            Species species1 = new Species(r);
            Species species2 = new Species(r);
            Species species3 = new Species(r);

            Genome gen1 = new Genome(r);
            gen1.OriginalFitness = fitness1;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(r);
            gen2.OriginalFitness = fitness2;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(r);
            gen3.OriginalFitness = fitness3;
            species3.addGenome(gen3);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3 });


            sim.orderSpeciesByOriginalFitness();

            Assert.IsTrue(sim.Species[0] == species1);
            Assert.IsTrue(sim.Species[1] == species2);
            Assert.IsTrue(sim.Species[2] == species3);
        }

        [TestMethod]
        public void OrderSpeciesTest_ReverseOrder()
        {
            Simulation sim = new Simulation(r, gen, 1);

            // Create some fake species and genomes
            Species species1 = new Species(r);
            Species species2 = new Species(r);
            Species species3 = new Species(r);

            Genome gen1 = new Genome(r);
            gen1.OriginalFitness = 0;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(r);
            gen2.OriginalFitness = 1;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(r);
            gen3.OriginalFitness = 2;
            species3.addGenome(gen3);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3 });


            sim.orderSpeciesByOriginalFitness();

            Assert.IsTrue(sim.Species[0] == species3);
            Assert.IsTrue(sim.Species[1] == species2);
            Assert.IsTrue(sim.Species[2] == species1);
        }

        [TestMethod]
        public void ObliterateWorstSpeciesTest()
        {
            Simulation sim = new Simulation(r, gen, 1);

            // Create some fake species and genomes
            Species species1 = new Species(r);
            Species species2 = new Species(r);
            Species species3 = new Species(r);

            Genome gen1 = new Genome(r);
            gen1.OriginalFitness = 0;
            species1.addGenome(gen1);
            species1.Age = 14;

            Genome gen2 = new Genome(r);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);
            species2.Age = 30;

            Genome gen3 = new Genome(r);
            gen3.OriginalFitness = 2;
            species3.addGenome(gen3);
            species3.Age = 30;

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3 });

            sim.orderSpeciesByOriginalFitness();

            sim.EpochID = 30;

            sim.PenalizeNonImprovingSpecies();

            Assert.AreEqual(false, species1.ShouldBePenalized);
            Assert.AreEqual(true, species2.ShouldBePenalized);
            Assert.AreEqual(false, species3.ShouldBePenalized);

        }

        [TestMethod]
        public void RemoveEmptySpeciesTest()
        {
            Simulation sim = new Simulation(r, gen, 1);

            // Create some fake species and genomes
            Species species1 = new Species(r);
            Species species2 = new Species(r);
            Species species3 = new Species(r);
            Species species4 = new Species(r);

            Genome gen1 = new Genome(r);
            gen1.OriginalFitness = 0;
            species1.addGenome(gen1);
            species1.Age = 14;

            Genome gen2 = new Genome(r);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);
            species2.Age = 30;

            Genome gen3 = new Genome(r);
            gen3.OriginalFitness = 2;
            species3.addGenome(gen3);
            species3.Age = 30;

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            Assert.AreEqual(4, sim.Species.Count);
            sim.RemoveEmptySpecies();
            Assert.AreEqual(3, sim.Species.Count);

            Assert.AreEqual(species1, sim.Species[0]);
            Assert.AreEqual(species2, sim.Species[1]);
            Assert.AreEqual(species3, sim.Species[2]);
        }

        [TestMethod()]
        public void HandlePopulationLevelStagnationTest_EvenPopulationSize()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 20);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);
            Species species2 = new Species(randomStub);
            Species species3 = new Species(randomStub);
            Species species4 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 10;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(randomStub);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(randomStub);
            gen3.OriginalFitness = 20;
            species3.addGenome(gen3);

            Genome gen4 = new Genome(randomStub);
            gen4.OriginalFitness = 1;
            species4.addGenome(gen4);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            sim.orderSpeciesByOriginalFitness();
            sim.HandlePopulationLevelStagnation();

            Assert.AreEqual(10, species1.Offspring);
            Assert.AreEqual(10, species1.ChampionOffspring);

            Assert.AreEqual(0, species2.Offspring);

            Assert.AreEqual(10, species3.Offspring);
            Assert.AreEqual(10, species3.ChampionOffspring);

            Assert.AreEqual(0, species4.Offspring);

            Assert.AreEqual(0, sim.GenerationsSinceLastUpdate);
        }

        [TestMethod()]
        public void HandlePopulationLevelStagnationTest_OddPopulationSize()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 21);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);
            Species species2 = new Species(randomStub);
            Species species3 = new Species(randomStub);
            Species species4 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 10;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(randomStub);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(randomStub);
            gen3.OriginalFitness = 20;
            species3.addGenome(gen3);

            Genome gen4 = new Genome(randomStub);
            gen4.OriginalFitness = 1;
            species4.addGenome(gen4);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            sim.orderSpeciesByOriginalFitness();
            sim.HandlePopulationLevelStagnation();

            Assert.AreEqual(10, species1.Offspring);
            Assert.AreEqual(10, species1.ChampionOffspring);

            Assert.AreEqual(0, species2.Offspring);

            Assert.AreEqual(11, species3.Offspring);
            Assert.AreEqual(11, species3.ChampionOffspring);

            Assert.AreEqual(0, species4.Offspring);

            Assert.AreEqual(0, sim.GenerationsSinceLastUpdate);
        }

        [TestMethod()]
        public void HandlePopulationLevelStagnationTest_SingleSpecies()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 20);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 10;
            species1.addGenome(gen1);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.Add(species1);

            sim.orderSpeciesByOriginalFitness();
            sim.HandlePopulationLevelStagnation();

            Assert.AreEqual(20, species1.Offspring);
            Assert.AreEqual(20, species1.ChampionOffspring);

            Assert.AreEqual(0, sim.GenerationsSinceLastUpdate);
        }

        [TestMethod()]
        public void CalculateSpeciesOffspringTest_TotalFitnessZero_Expected_AllSpeciesHaveSameOffspring()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 20);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);
            Species species2 = new Species(randomStub);
            Species species3 = new Species(randomStub);
            Species species4 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 10;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(randomStub);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(randomStub);
            gen3.OriginalFitness = 20;
            species3.addGenome(gen3);

            Genome gen4 = new Genome(randomStub);
            gen4.OriginalFitness = 1;
            species4.addGenome(gen4);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            sim.orderSpeciesByOriginalFitness();
            sim.CalculateSpeciesOffspring(0.0f);

            foreach (Species species in sim.Species)
                Assert.AreEqual(5, species.Offspring);
        }

        [TestMethod()]
        public void CalculateSpeciesOffspringTest_TotalFitnessZero_Expected_AllSpeciesHaveSameOffspring_ButOne()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 22);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);
            Species species2 = new Species(randomStub);
            Species species3 = new Species(randomStub);
            Species species4 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 10;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(randomStub);
            gen2.OriginalFitness = 0;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(randomStub);
            gen3.OriginalFitness = 20;
            species3.addGenome(gen3);

            Genome gen4 = new Genome(randomStub);
            gen4.OriginalFitness = 1;
            species4.addGenome(gen4);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            sim.orderSpeciesByOriginalFitness();
            sim.CalculateSpeciesOffspring(0.0f);

            Assert.AreEqual(7, sim.Species[0].Offspring);
            for(int i = 1; i < sim.Species.Count; ++i)
                Assert.AreEqual(5, sim.Species[i].Offspring);
        }

        [TestMethod()]
        public void CalculateSpeciesOffspringTest_TotalFitnessGreaterThanZero()
        {
            UnitTests.RandomStub randomStub = new UnitTests.RandomStub();
            Simulation sim = new Simulation(randomStub, gen, 21);

            // Create some fake species and genomes
            Species species1 = new Species(randomStub);
            Species species2 = new Species(randomStub);
            Species species3 = new Species(randomStub);
            Species species4 = new Species(randomStub);

            Genome gen1 = new Genome(randomStub);
            gen1.OriginalFitness = 5;
            gen1.Fitness = 5;
            species1.addGenome(gen1);

            Genome gen2 = new Genome(randomStub);
            gen2.OriginalFitness = 10;
            gen2.Fitness = 10;
            species2.addGenome(gen2);

            Genome gen3 = new Genome(randomStub);
            gen3.OriginalFitness = 15;
            gen3.Fitness = 15;
            species3.addGenome(gen3);

            Genome gen4 = new Genome(randomStub);
            gen4.OriginalFitness = 20;
            gen4.Fitness = 20;
            species4.addGenome(gen4);

            // Remove default species/genomes and add fake ones
            sim.Species.Clear();
            sim.Species.AddRange(new Species[] { species1, species2, species3, species4 });

            sim.orderSpeciesByOriginalFitness();
            sim.CalculateSpeciesOffspring(50.0f);

            Assert.AreEqual(2, species1.Offspring);
            Assert.AreEqual(4, species2.Offspring);
            Assert.AreEqual(6, species3.Offspring);
            Assert.AreEqual(9, species4.Offspring);
        }
    }
}