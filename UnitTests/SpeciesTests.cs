﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NEAT_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp.Tests
{
    [TestClass()]
    public class SpeciesTests
    {
        Random r = null;
        Species species = null;
        Simulation sim = null;

        [TestInitialize]
        public void SetupTest()
        {
            r = new Random(0);

            Genome gen = new Genome(r);
            sim = new Simulation(r, gen, 1);

            // Create some fake species and genomes
            species = new Species(r);
            species.ParentSimulation = sim;
        }

        [TestCleanup]
        public void CleanupTest()
        {
            r = null;
            species = null;
            sim = null;
        }

        [TestMethod]
        public void getChampionTest_BeforeFitnessAdjustment()
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 1.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });

            Assert.IsTrue(species.getChampion() == gen3);
        }

        [TestMethod]
        public void getChampionTest_BeforeFitnessAdjustment_TwoGenomesWithSameFitness()
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 3.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });

            Assert.IsTrue(species.getChampion() == gen1);
        }

        [TestMethod]
        public void getChampionTest_AfterFitnessAdjustment()
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 1.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });
            species.adjustFitness();

            Assert.IsTrue(species.getChampion() == gen3);
        }

        [TestMethod]
        public void adjustFitnessTest()
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 1.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });

            float epsilon = 0.0001f;

            double avgFitness = species.getAverageFitness();
            Assert.IsTrue(Math.Abs(species.getAverageFitness() - 2.0f) < epsilon, String.Format("Expected {0}, got {1}", 2.0f, avgFitness));

            species.adjustFitness();

            // Check if Fitness has correct values after adjustment
            Assert.IsTrue(Math.Abs(gen1.Fitness - 1.0f / 3.0f) < epsilon);
            Assert.IsTrue(Math.Abs(gen2.Fitness - 2.0f / 3.0f) < epsilon);
            Assert.IsTrue(Math.Abs(gen3.Fitness - 3.0f / 3.0f) < epsilon);

            // Check if Genomes are correctly ordered
            Assert.IsTrue(species.Genomes[0] == gen3);
            Assert.IsTrue(species.Genomes[1] == gen2);
            Assert.IsTrue(species.Genomes[2] == gen1);

            avgFitness = species.getAverageFitness();
            Assert.IsTrue(Math.Abs(species.getAverageFitness() - 2.0f / 3.0f) < epsilon, String.Format("Expected {0}, got {1}", 2.0f / 3.0f, avgFitness));
        }

        [TestMethod]
        public void getSampleGenomeTest()
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 1.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });

            Assert.IsTrue(species.getSampleGenome() == gen1);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(3)]
        [DataRow(10)]
        public void reproduceTest(int offspring)
        {
            Genome gen1 = new Genome(r); gen1.ParentSimulation = sim; gen1.Fitness = 1.0f;
            Genome gen2 = new Genome(r); gen2.ParentSimulation = sim; gen2.Fitness = 2.0f;
            Genome gen3 = new Genome(r); gen3.ParentSimulation = sim; gen3.Fitness = 3.0f;

            species.Genomes.AddRange(new Genome[] { gen1, gen2, gen3 });
            species.Offspring = offspring;

            List<Genome> nextGeneration = new List<Genome>();
            List<Innovation> innovations = new List<Innovation>();
            species.reproduce(nextGeneration, innovations);

            Assert.AreEqual(offspring, nextGeneration.Count);
        }
    }
}