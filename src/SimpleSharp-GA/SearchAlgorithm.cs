using System;
using System.Diagnostics;

namespace SimpleSharp_GA
{

    #region GA
    public class SearchAlgorithms
    {
        public static Random rnd = new Random();
        public static double[,] GeneticAlgorithm(
            long runtime,
            int depth,
            int size,
            int populationSize,
            int mutationChildren,
            int crossoverChildren,
            Func<Solution, double> evalFunction,
            double[,] initialSolution,
            Stopwatch stopwatch)
        {
            if (mutationChildren + crossoverChildren > populationSize)
            {
                throw new ArgumentException("Can't have more mutations and crossovers than populationSize");
            }

            stopwatch = stopwatch ?? new Stopwatch();

            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            var i = 0;
            if (initialSolution == null)
            {
                initialSolution = CreateRandom(depth, size);
            }

            var population = new Solution[populationSize];
            population[0] = new Solution(initialSolution);
            population[0].Evaluation = evalFunction(population[i]);
            for (i = 1; i < populationSize; i++)
            {
                population[i] = new Solution(CreateRandom(depth, size));
                population[i].Evaluation = evalFunction(population[i]);
            }

            var placeHolderSolutions = new Solution[populationSize];
            for (i = 0; i < populationSize; i++)
            {
                placeHolderSolutions[i] = new Solution(new double[depth, size]);
            }

            //Search loop
            var roundtime = stopwatch.ElapsedMilliseconds;
            long maxRoundTime = 0;
            while (runtime < roundtime)
            {
                //Elite child
                var best = population[0];
                var bestPos = 0;
                for (i = 1; i < populationSize; i++)
                {
                    if (population[i].Evaluation > best.Evaluation)
                    {
                        best = population[i];
                        bestPos = i;
                    }
                }
                WriteTo(best.Data, placeHolderSolutions[0].Data, depth, size);
                placeHolderSolutions[0].Evaluation = best.Evaluation;

                //Mutate
                var max = mutationChildren + 1;
                for (i = 1; i < max; i++)
                {
                    Mutate(best.Data, placeHolderSolutions[i].Data, 1, depth, size);
                    best = population[rnd.Next(0, populationSize)];
                }

                //CrossOver
                max = i + crossoverChildren;
                for (; i < max; i++)
                {
                    var f = rnd.NextDouble() < 0.2 ? bestPos : rnd.Next(0, populationSize);
                    var s = rnd.Next(0, populationSize);
                    while (s == f) s = rnd.Next(0, populationSize);
                    CrossOver(population[f].Data, population[s].Data, placeHolderSolutions[i].Data, depth, size);
                }

                //Random
                max = populationSize;
                for (; i < max; i++)
                {
                    Randomize(placeHolderSolutions[i].Data, depth, size);
                }

                //Evaluation
                for (i = 1; i < populationSize; i++)
                {
                    placeHolderSolutions[i].Evaluation = evalFunction(placeHolderSolutions[i]);
                }

                //Reset
                var temp = population;
                population = placeHolderSolutions;
                placeHolderSolutions = temp;

                var currentRound = stopwatch.ElapsedMilliseconds;
                if (currentRound - roundtime > maxRoundTime) maxRoundTime = currentRound - roundtime;
                roundtime = currentRound;
                if (runtime - maxRoundTime < roundtime) break;
            }

            var currentBest = FindBestSolution(population);
            var tempMutationTarget = population[1] == currentBest ? population[2] : population[1];
            while (runtime > roundtime)
            {
                for (int j = 0; j < 3; j++)
                {
                    Mutate(currentBest.Data, tempMutationTarget.Data, 1, depth, size);
                    tempMutationTarget.Evaluation = evalFunction(tempMutationTarget);
                    if (tempMutationTarget.Evaluation > currentBest.Evaluation)
                    {
                        var temp = currentBest;
                        currentBest = tempMutationTarget;
                        tempMutationTarget = temp;
                    }
                }

                roundtime = stopwatch.ElapsedMilliseconds;
            }

            Console.Error.WriteLine("Best score: " + currentBest.Evaluation);
            return currentBest.Data;
        }
        private static Solution FindBestSolution(Solution[] population)
        {
            var best = population[0];
            for (var i = 1; i < population.Length; i++)
            {
                if (population[i].Evaluation > best.Evaluation)
                {
                    best = population[i];
                }
            }

            return best;
        }
        private static void CrossOver(double[,] fromSolution1, double[,] fromSolution2, double[,] intoSolution, int depth, int size)
        {
            var swapPoint = rnd.Next(0, depth);
            var side = rnd.Next(0, 3);
            var target = rnd.Next(0, 2) == 0 ? fromSolution1 : fromSolution2;
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    intoSolution[i, j] = target[i, j];
                }
                if (i < swapPoint)
                {
                    for (int j = 2 * side; j < side * 2 + 2; j++)
                    {
                        intoSolution[i, j] = fromSolution1[i, j];
                    }
                }
                else
                {
                    for (int j = 2 * side; j < side * 2 + 2; j++)
                    {
                        intoSolution[i, j] = fromSolution2[i, j];
                    }
                }
            }
        }
        private static void WriteTo(double[,] fromSolution, double[,] intoSolution, int depth, int size)
        {
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    intoSolution[i, j] = fromSolution[i, j];
                }
            }
        }
        private static void Mutate(double[,] fromSolution, double[,] intoSolution, double amplitude, int depth, int size)
        {
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (rnd.NextDouble() < 0.15)
                    {
                        intoSolution[i, j] = rnd.NextDouble();
                    }
                    else
                    {
                        intoSolution[i, j] = fromSolution[i, j];
                    }
                }
            }
        }
        private static void Randomize(double[,] solution, int depth, int size)
        {
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    solution[i, j] = rnd.NextDouble();
                }
            }
        }
        private static double[,] CreateRandom(int depth, int size)
        {
            var solution = new double[depth, size];
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    solution[i, j] = rnd.NextDouble();
                }
            }
            return solution;
        }
    }


    public class Solution
    {
        public Solution(double[,] data)
        {
            Data = data;
        }

        public Solution(int depth, int size)
        {
            Data = new double[depth, size];
        }

        public double? Evaluation;
        public double[,] Data;
    }
    #endregion
}