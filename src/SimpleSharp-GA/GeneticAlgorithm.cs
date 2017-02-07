using System;
using System.Linq;
using System.Diagnostics;

namespace SimpleSharp_GA
{
	public class GeneticAlgorithm
	{
		public static Solution[] FindBestSolutions(
			  int runTime, 
	          int populationCount, 
	          int depth, 
	          int solutionSize, 
	          Func<Solution, double> evaluation, 
	          Solution[] initialSolution)
		{
			var ga = new GeneticAlgorithm(depth, solutionSize, populationCount, evaluation);
			ga.AddInitial(initialSolution);
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < runTime)
			{
				ga.CreateNextGeneration(1 - stopwatch.ElapsedMilliseconds / runTime);
			}
			return ga.GetSortedResult();
		}

		private readonly int _eliteChildrenCount;
		private readonly int _crossOverCount;
		private readonly int _mutationCount;

		private readonly int _depth;
		private readonly int _size;
		private readonly int _populationSize;
		private readonly Func<Solution, double> _evaluation;
		private readonly Random _rnd = new Random();
		private readonly Solution[] _solutions;

		private GeneticAlgorithm(int depth, int size, int populationSize, Func<Solution, double> evaluation)
		{
			_depth = depth;
			_size = size;
			_evaluation = evaluation;
			_populationSize = populationSize;
			_eliteChildrenCount = (int)Math.Truncate(populationSize * 0.1 + 1);
			_crossOverCount = (int)Math.Truncate(populationSize * 0.6);
			_mutationCount = populationSize - _eliteChildrenCount - _crossOverCount;
			_solutions = new Solution[populationSize];
		}

		private void AddInitial(Solution[] initialSolution)
		{
			for (int i = 0; i < initialSolution.Length && i < _solutions.Length; i++)
			{
				_solutions[i] = initialSolution[i];
			}
			if (_solutions.Length > initialSolution.Length)
			{
				for (int i = _solutions.Length - initialSolution.Length; i < _solutions.Length; i++)
				{
					_solutions[i] = CreateRandom();
				}
			}
		}

		private Solution[] GetSortedResult()
		{
			return _solutions.OrderByDescending(s => s.Evaluation).ToArray();
		}

		private void CreateNextGeneration(double amplitude)
		{
			//TODO:
		}

		private int GetRandom(int min, int max, int exclusive)
		{
			var number = _rnd.Next(min, max);
			while (number == exclusive)
				number = _rnd.Next(min, max);
			return number;
		}

		private Solution CreateRandom()
		{
			var solution = new Solution(_depth, _size);
			return solution;
		}
	}

	public class Solution
	{
		public Solution(int depth, int size)
		{
			Data = new double[depth,size];
		}

		public double Evaluation;
		public double[,] Data;
	}
}
