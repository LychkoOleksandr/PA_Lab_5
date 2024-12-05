class Program
{
    private const int PopulationSize = 50;
    private const int MaxGenerations = 100;
    private const double CrossoverRate = 1;
    private const double MutationRate = 0.1;
    private const int CliqueSize = 3;

    private static void Main()
    {
        var graph = GenerateGraph(300, 2, 30);
        Console.WriteLine("Graph generated with 300 nodes.");

        var population = InitializePopulation(graph.Count, CliqueSize);
        int bestFitness = 0;

        for (int generation = 0; generation < MaxGenerations; generation++)
        {
            List<int> fitness = EvaluateFitness(population, graph);

            bestFitness = fitness.Max();
            Console.WriteLine($"Generation {generation + 1}: Best Fitness = {bestFitness}");

            if (bestFitness == CliqueSize)
            {
                Console.WriteLine("Target clique size found!");
                List<int> bestSolution = population
                    .OrderByDescending(ind => EvaluateFitness([ind], graph)[0]).First();
                Console.WriteLine($"Clique Found: {string.Join(",", bestSolution)}");
                Console.WriteLine($"Clique Size: {bestSolution.Count}");
                break;
            }

            var offspring = Crossover(population);
            Mutate(offspring, graph.Count);
            LocalSearch(offspring, graph);

            population = FormNewPopulation(population, offspring, graph);
        }

        if (bestFitness != CliqueSize)
        {
            Console.WriteLine("Target clique size not found.");
        }
    }

    private static List<List<int>> GenerateGraph(int numNodes, int minDegree, int maxDegree)
    {
        Random rand = new Random();
        var graph = Enumerable.Range(0, numNodes).Select(_ => new List<int>()).ToList();

        for (int i = 0; i < numNodes; i++)
        {
            int degree = Math.Min(rand.Next(minDegree, maxDegree + 1), numNodes - 1);

            while (graph[i].Count < degree)
            {
                int neighbor = rand.Next(numNodes);
                if (neighbor != i && !graph[i].Contains(neighbor))
                {
                    graph[i].Add(neighbor);
                    graph[neighbor].Add(i);
                }
            }
        }

        return graph;
    }

    private static List<List<int>> InitializePopulation(int numNodes, int cliqueSize)
    {
        Random rand = new Random();
        var population = new List<List<int>>();

        for (int i = 0; i < PopulationSize; i++)
        {
            var individual = new HashSet<int>();
            while (individual.Count < cliqueSize)
            {
                individual.Add(rand.Next(numNodes));
            }
            population.Add(individual.ToList());
        }

        return population;
    }

    private static List<int> EvaluateFitness(List<List<int>> population, List<List<int>> graph)
    {
        return population.Select(ind =>
        {
            List<int> largestClique = FindLargestClique(ind, graph);
            return largestClique.Count; 
        }).ToList();
    }

    private static List<int> FindLargestClique(List<int> individual, List<List<int>> graph)
    {
        var subsets = GetAllSubsets(individual);

        List<int> largestClique = new List<int>();
        foreach (var subset in subsets)
        {
            if (IsClique(subset, graph) && subset.Count > largestClique.Count)
            {
                largestClique = subset;
            }
        }

        return largestClique;
    }

    private static List<List<int>> GetAllSubsets(List<int> set)
    {
        var subsets = new List<List<int>>();
        int subsetCount = 1 << set.Count; 

        for (int i = 0; i < subsetCount; i++)
        {
            List<int> subset = new List<int>();
            for (int j = 0; j < set.Count; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    subset.Add(set[j]);
                }
            }
            subsets.Add(subset);
        }

        return subsets;
    }

    private static bool IsClique(List<int> individual, List<List<int>> graph)
    {
        return individual.All(node => individual.All(other => node == other || graph[node].Contains(other)));
    }

    private static List<List<int>> Crossover(List<List<int>> population)
    {
        Random rand = new Random();
        var offspring = new List<List<int>>();

        for (int i = 0; i < PopulationSize / 2; i++)
        {
            if (!(rand.NextDouble() < CrossoverRate)) continue;
            List<int> parent1 = population[rand.Next(PopulationSize)];
            List<int> parent2 = population[rand.Next(PopulationSize)];

            int minLength = Math.Min(parent1.Count, parent2.Count);
            int point = rand.Next(1, minLength);

            List<int> child = parent1.Take(point).Concat(parent2.Skip(point)).Distinct().ToList();
            offspring.Add(child);
        }

        return offspring;
    }

    private static void Mutate(List<List<int>> offspring, int numNodes)
    {
        Random rand = new Random();

        foreach (var individual in offspring)
        {
            if (!(rand.NextDouble() < MutationRate)) continue;
            int index = rand.Next(individual.Count);
            int newNode;
            do
            {
                newNode = rand.Next(numNodes);
            } while (individual.Contains(newNode));

            individual[index] = newNode;
        }
    }

    private static List<List<int>> FormNewPopulation(List<List<int>> population, List<List<int>> offspring, List<List<int>> graph)
    {
        var combined = population.Concat(offspring).ToList();
        List<int> combinedFitness = EvaluateFitness(combined, graph);

        return combinedFitness
            .Select((fit, idx) => (fit, idx))
            .OrderByDescending(x => x.fit)
            .Take(PopulationSize)
            .Select(x => combined[x.idx])
            .ToList();
    }
    
    private static void LocalSearch(List<List<int>> offspring, List<List<int>> graph)
    {
        foreach (var individual in offspring)
        {
            Swap(individual, graph);
            AddVertexToClique(individual, graph);
        }
    }

    private static void Swap(List<int> clique, List<List<int>> graph)
    {
        Random rand = new Random();
        int index1 = rand.Next(clique.Count);
        int index2 = rand.Next(clique.Count);

        (clique[index1], clique[index2]) = (clique[index2], clique[index1]);
    }

    private static void AddVertexToClique(List<int> clique, List<List<int>> graph)
    {
        for (int i = 0; i < graph.Count; i++)
        {
            if (!clique.Contains(i) && IsClique(clique.Concat([i]).ToList(), graph))
            {
                clique.Add(i);
                break;
            }
        }
    }
}
