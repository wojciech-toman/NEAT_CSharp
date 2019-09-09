using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT_CSharp
{
	// Main class of the simulation.
	// To set up a simulation you need to call constructor and pass it initial genome (initial neural network). Then you can
	// advance the simulation by calling epoch() method.
	// TODO: add option to perform extinction of all species due to stagnation and then recreate population with new
	// random genomes?
	public class Simulation
	{
        private List<Species> species = new List<Species>();
        private List<Genome> nextGeneration = new List<Genome>();
        private List<Innovation> innovations = new List<Innovation>();
        private int populationSize = 0;
		private double highestFitness = 0.0;
		private int generationsSinceLastUpdate;
        private Random random = null;


		public SimulationParameters Parameters { get; set; } = new SimulationParameters();


		// Spawn population from a single genome (this will be initial state) and with positive populationSize number of organisms.
		public Simulation(Random rnd, Genome basicGenome, int populationSize)
		{
			if (basicGenome == null) throw new ArgumentNullException(nameof(basicGenome));
            if (populationSize <= 0) throw new ArgumentException("Population size has to be positive number");

			this.populationSize = populationSize;
			this.random = rnd;

			for (int i = 0; i < populationSize; ++i)
			{
				Genome g = basicGenome.copy();
				g.ParentSimulation = this;
				g.mutateWeights(1.0f);
				this.nextGeneration.Add(g);
			}

			if (basicGenome.ConnectionGenes.Count > 0)
				Innovation.SetCurrentID(basicGenome.ConnectionGenes[basicGenome.ConnectionGenes.Count - 1].Innovation);
			else
				Innovation.SetCurrentID(0);


			// Add all new genomes to species
			this.addGenomesToSpecies(nextGeneration);
			this.orderSpecies();
		}

		public void orderSpecies()
		{
			this.species.Sort((x, y) => (y.Genomes[0].OriginalFitness.CompareTo(x.Genomes[0].OriginalFitness)));
		}

        public int EpochID { get; set; } = 0;

        public List<Species> Species { get { return this.species; } }

		public List<Genome> Genomes
		{
			get
			{
				return this.nextGeneration;
			}
		}

		// Advances the NEAT simulation to the next step. This involves several steps:
		// 1. Penalize the species that didn't improve for a long time - the idea is to minimize their chances of getting offspring
		// 2. Remove genomes marked for deletion (stagnant)
		// 3. Reproduce the species (genome's offspring count is based on its fitness function result)
		public void epoch()
        {
            if (this.Genomes.Count > this.populationSize)
            {
                throw new ArgumentException($"Invalid number of genomes: {Genomes.Count}. Should be {populationSize}");
            }

            List<Genome> previousGeneration = new List<Genome>(this.nextGeneration);


            // Adjust fitness of each species and also sort genomes
            foreach (Species s in this.species)
            {
                s.Age++;
                s.adjustFitness();
                s.Innovations = this.innovations;
            }

            // Now order species - it's essential that genomes are sorted before that (what's done above)
            this.orderSpecies();


            // Once in 30 epochs, flag the worst performing species with age over 20 to be obliterated (its genomes will
            // get big fitness penalty for not improving). This should help reduce stagnation
            if (this.EpochID > 0 && this.EpochID % (this.Parameters.MaxSpeciesGenerationsWithoutImprovement * 2) == 0)
                this.ObliterateWorstSpecies();

            // Clear nextGeneration list -> new organisms will be added to it
            nextGeneration.Clear();

            double totalPopulationFitness = 0.0;
            foreach (Species s in this.species)
            {
                totalPopulationFitness += s.getAverageFitness();
            }

            // Remove all species that didn't improve for specified number of generations (but make sure to leave
            // 1 species left)
            /*int index = this.species.FindIndex(item => item.AgeWithoutImprovement >= maxSpeciesGenerationsWithoutImprovement);
			while (this.species.Count > 1 && index != -1)
			{
				this.species.RemoveAt(index);
				index = this.species.FindIndex(item => item.AgeWithoutImprovement >= maxSpeciesGenerationsWithoutImprovement);
			}*/

            // Calculate number of offspring for each species proportionally to species adjusted fitness
            int organismsLeft = this.populationSize;
            if (totalPopulationFitness > 0.0f)
            {
                foreach (Species s in this.species)
                {
                    s.Offspring = (int)(s.getAverageFitness() / totalPopulationFitness * populationSize);
                    organismsLeft -= s.Offspring;
                }
                // If we have some offspring left, then add it to the best performing species
                if (organismsLeft > 0) this.species[0].Offspring += organismsLeft;
                // If we added too many organisms, subtract them from the worst performing species
                if (organismsLeft < 0)
                {
                    for (int i = this.species.Count - 1; i > 0; --i)
                    {
                        this.species[i].Offspring += organismsLeft;
                        if (this.species[i].Offspring >= 0)
                            break;
                        // Remove some more from other species
                        else
                        {
                            organismsLeft -= this.species[i].Offspring;
                            this.species[i].Offspring = 0;
                        }
                    }
                }
            }
            else
            {
                // Handle very special case when total fitness of whole population is 0 - add same number of offspring to all species
                int toAdd = this.populationSize / this.species.Count;
                foreach (Species s in this.species)
                {
                    s.Offspring = toAdd;
                    organismsLeft -= s.Offspring;
                }
                // If we have some offspring left, then add it to the best performing species
                if (organismsLeft > 0) this.species[0].Offspring += organismsLeft;
                // If we added too many organisms, subtract them from the worst performing species
                if (organismsLeft < 0)
                {
                    for (int i = this.species.Count - 1; i > 0; --i)
                    {
                        this.species[i].Offspring += organismsLeft;
                        if (this.species[i].Offspring >= 0)
                            break;
                        // Remove some more from other species
                        else
                        {
                            organismsLeft -= this.species[i].Offspring;
                            this.species[i].Offspring = 0;
                        }
                    }
                }
            }

            // Check for population-level stagnation
            double curBestFitness = this.species[0].Genomes[0].OriginalFitness;
            this.species[0].Genomes[0].IsPopulationChampion = true;
            if (curBestFitness > this.highestFitness)
            {
                this.highestFitness = curBestFitness;
                this.generationsSinceLastUpdate = 0;
            }
            else
                ++this.generationsSinceLastUpdate;

            // If whole population is stagnant we let to reproduce just 2 best species. Also, we generate
            // special children from both species champions
            if (this.generationsSinceLastUpdate > this.Parameters.MaxGeneralGenerationsWithoutImprovement)
            {
                this.generationsSinceLastUpdate = 0;

                int halfPopulationSize = this.populationSize / 2;
                this.species[0].LastImprovementAge = this.species[0].Age;

                if (this.species.Count > 1)
                {
                    this.species[0].Offspring = halfPopulationSize;
                    this.species[0].ChampionOffspring = halfPopulationSize;

                    this.species[1].Offspring = this.populationSize - halfPopulationSize;
                    this.species[1].ChampionOffspring = halfPopulationSize;
                    this.species[1].LastImprovementAge = this.species[1].Age;

                    for (int i = 2; i < this.species.Count; ++i)
                    {
                        this.species[i].Offspring = 0;
                    }
                }
                else
                {
                    this.species[0].Offspring = this.populationSize;
                    this.species[0].ChampionOffspring = this.populationSize;
                }
            }


            // Remove genomes that were marked for deletion so they won't reproduce
            foreach (Genome g in previousGeneration)
            {
                if (g.ShouldBeEliminated)
                {
                    g.Species.removeGenome(g);
                }
            }

            // Reproduce the species
            foreach (Species s in this.species)
            {
                s.reproduce(nextGeneration, this.innovations);
            }
            this.addGenomesToSpecies(nextGeneration);

            // Remove previous generation from the species
            foreach (Genome gen in previousGeneration)
            {
                gen.Species.removeGenome(gen);
            }

            // Remove all empty species (unless there is only 1 species left)
            this.RemoveEmptySpecies();

            foreach (Species s in this.species)
                s.orderGenomes();
            this.orderSpecies();

            // Remove innovations of this generation
            this.innovations.Clear();

            // Finally increase epoch counter
            ++this.EpochID;
        }

        public void RemoveEmptySpecies()
        {
            if (this.species.Count > 1)
                this.species.RemoveAll(item => item.Genomes.Count == 0);
        }

        public void ObliterateWorstSpecies()
        {
            for (int i = this.species.Count - 1; i >= 0; --i)
            {
                if (species[i].Age >= (this.Parameters.MaxSpeciesGenerationsWithoutImprovement + 5))
                {
                    species[i].ShouldBeObliterated = true;
                    break;
                }
            }
        }

        private void addGenomesToSpecies(List<Genome> genomes)
		{
			foreach (Genome gen in genomes)
			{
				bool compatibleSpeciesFound = false;
				foreach (Species s in this.species)
				{
					if (gen.compatibilityDistance(s.getSampleGenome()) < this.Parameters.CompatibilityThreshold)
					{
						s.addGenome(gen);
						gen.Species = s;
						compatibleSpeciesFound = true;
						break;
					}
				}
				if (!compatibleSpeciesFound)
				{
					Species newSpecies = new Species(this.random);
					newSpecies.ParentSimulation = this;
					newSpecies.addGenome(gen);
					gen.Species = newSpecies;
					this.species.Add(newSpecies);
				}
			}
		}
	}
}
