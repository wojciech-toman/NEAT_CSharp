namespace NEAT_CSharp
{
	// Parameters of Simulation object. Use it to tune behaviour of the simulation
	public class SimulationParameters
	{
		public float MutateWithoutCrossover { get; set; } = 0.25f;              // Probability of doing only mutation without crossover
		public float MateWithoutMutatingProbability { get; set; } = 0.2f;       // Probability of a simple crossover, without mutating the resulting child
		public float AddConnectionProbability { get; set; } = 0.05f;            // Can be 0.3 for large populations
		public float AddNodeProbability { get; set; } = 0.03f;                  // Probability of adding new node as a mutation
		public float MutateConnectionWeightsProbability { get; set; } = 0.8f;   // Probability of mutating connection weights
		public float InterspeciesMateRate { get; set; } = 0.001f;               // Rate of crossovers between different species
		public float SurvivalThreshold { get; set; } = 0.2f;                    // Percentage of genomes in the species (best ones only) that can reproduce further
		public float AverageCrossoverProbability { get; set; } = 0.4f;          // Probability that weights of the genes will be averaged
		public float WeightMutationPower { get; set; } = 2.5f;                  // Maximum value by which connection weight can change as a result of mutation
		public float SpeciesStagnationPenalty { get; set; } = 0.01f;            // Extreme penalty for species not improving for too long

		public float MutateToggleEnabledProbability { get; set; } = 0.0f;       // Probability of changing 'enabled' status of a connection gene
		public float MutateReenableProbability { get; set; } = 0.0f;            // Probability of reenabling previously disabled connection gene
		public float RecurrencyProbability { get; set; } = 0.0f;                // Probability of adding recurrent link

		// Coefficients used by the compatibility function
		public float c1 { get; set; } = 1.0f;
		public float c2 { get; set; } = 1.0f;
		public float c3 { get; set; } = 0.4f;


		//const float uniformPerturbProbability = 0.9f;
		//const float perturbationRate = 0.3f;
		public bool AreConnectionWeightsCapped { get; set; } = true;            // Tells whether connection weight bound is used (if not, weights can take arbitrary value)
		public float MaxWeight { get; set; } = 8.0f;                            // Maximum value a connection weight can take
		public float DisableGeneProbability { get; set; } = 0.75f;              // Probability of disabling connection gene if it was disabled in either of parents

		public float CompatibilityThreshold { get; set; } = 3.0f;               // Compatibility threshold used when comparing 2 genomes for similarity
		public int MaxSpeciesGenerationsWithoutImprovement { get; set; } = 15;  // Number of epochs/generations after which species is considered stagnant (and should be penalized)
		public int MaxGeneralGenerationsWithoutImprovement { get; set; } = 20;  // Number of epochs/generations after which whole population is considered stagnant
		public int PopulationExtinctionLimit { get; set; } = 100;               // Number of epochs/generations after which whole population is extinct and new population is spawned
	}
}
