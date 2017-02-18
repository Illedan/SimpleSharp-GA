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
			  Action<Solution, double> mutate,
	          Solution[] initialSolution)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var ga = new GeneticAlgorithm(depth, solutionSize, populationCount, evaluation, mutate);
			ga.AddInitial(initialSolution);
			while (stopwatch.ElapsedMilliseconds < runTime)
			{
				ga.CreateNextGeneration(1 - (double)stopwatch.ElapsedMilliseconds / (double)runTime);
			}
			return ga.GetSortedResult();
		}

		private readonly int _crossOverCount;
		private readonly int _mutationCount;

		private readonly int _depth;
		private readonly int _size;
		private readonly int _populationSize;
		private readonly Func<Solution, double> _evaluation;
		private readonly Action<Solution, double> _mutate;
		private readonly Random _rnd = new Random();
		private Solution[] _solutions;
		private Solution[] _nextSolutions;

		private GeneticAlgorithm(int depth, 
		                         int size, 
		                         int populationSize, 
		                         Func<Solution, double> evaluation, 
		                         Action<Solution, double> mutate)
		{
			_mutate = mutate;
			_depth = depth;
			_size = size;
			_evaluation = evaluation;
			_populationSize = populationSize;
			_crossOverCount = (int)Math.Truncate(populationSize * 0.6);
			_mutationCount = populationSize - 2 - _crossOverCount;
			_solutions = new Solution[populationSize];
			_nextSolutions = new Solution[populationSize];
		}

		private void AddInitial(Solution[] initialSolution)
		{
			for (int i = 0; i < initialSolution.Length && i < _solutions.Length; i++)
			{
				_solutions[i] = initialSolution[i];
			}
			if (_solutions.Length > initialSolution.Length)
			{
				for (int i = initialSolution.Length; i < _solutions.Length; i++)
				{
					_solutions[i] = CreateRandom();
				}
			}
			for (k = 0; k < _populationSize; k++)
			{
				_solutions[k].Evaluation = _evaluation(_solutions[k]);
			}
		}

		private Solution[] GetSortedResult()
		{
			return _solutions.OrderByDescending(s => s.Evaluation).ToArray();
		}

		private int k;
		private void CreateNextGeneration(double amplitude)
		{
			_nextSolutions[0] = FindBest(null);
			_nextSolutions[1] = FindBest(_nextSolutions[0]);
			for (int i = 0; i < _mutationCount; i++)
			{
				var clone = _solutions[_rnd.Next(0, _populationSize)].Clone(_depth, _size);
				_mutate(clone, amplitude);
				_nextSolutions[i + 2] = clone;
			}
			for (int i = 0; i < _crossOverCount; i++)
			{
				_nextSolutions[i + 2 + _mutationCount] = CrossOver(_solutions[_rnd.Next(0, _populationSize)], _solutions[_rnd.Next(0, _populationSize)]);   
			}

			_solutions = _nextSolutions;
			for (k = 0; k < _populationSize; k++)
			{
				if(_solutions[k].Evaluation==null)_solutions[k].Evaluation = _evaluation(_solutions[k]);
			}
		}

		private Solution FindBest( Solution except)
		{
			var current = double.MinValue;
			Solution found = null;
			for (k = 0; k < _populationSize; k++)
			{
				if (_solutions[k].Evaluation > current && except != _solutions[k])
				{
					current = _solutions[k].Evaluation.Value;
					found = _solutions[k];
				}
			}
			return found;
		}

		private int GetRandom(int min, int max, int exclusive)
		{
			var number = _rnd.Next(min, max);
			while (number == exclusive)
				number = _rnd.Next(min, max);
			return number;
		}

		private Solution CrossOver(Solution sol1, Solution sol2)
		{
			sol1 = sol1.Clone(_depth, _size);
			for (int i = 0; i < _depth; i++)
			{
				for (var l = 0; l < _size; l++)
				{
					if (_rnd.Next(0, 2) == 1) sol1.Data[i, l] = sol2.Data[i, l];
				}
			}
			return sol1;
		}

		private Solution CreateRandom()
		{
			var solution = new Solution(_depth, _size);
			for (int i = 0; i < _depth; i++)
			{
				for (int l = 0; l < _size; l++)
				{
					solution.Data[i, l] = _rnd.NextDouble();
				}
			}
			return solution;
		}
	}

	public class Solution
	{
		public Solution Clone(int depth, int size)
		{
			var sol = new Solution(depth, size);
			for (int i = 0; i < depth; i++)
			{
				for (int l = 0; l < size; l++)
				{
					sol.Data[i, l] = Data[i, l];
				}
			}
			return sol;
		}
		public Solution(int depth, int size)
		{
			Data = new double[depth,size];
		}

		public double? Evaluation;
		public double[,] Data;
	}
}
