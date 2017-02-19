using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleSharp_GA
{
	public interface ISolutionDefinition
	{
		/// <summary>
		/// Should return a new completly random solution inside this solutionspace
		/// </summary>
		/// <returns>The random.</returns>
		Solution CreateRandom(int depth, int solutionSize);
		/// <summary>
		/// Evaluation the specified sol.
		/// </summary>
		/// <param name="sol">Sol.</param>
		double Evaluation(Solution sol);

		/// <summary>
		/// Should mutate the fromSolution into the into Solution. Do not alter fromSolution.
		/// None of the values can be null.
		/// </summary>
		/// <param name="fromSolution">From solution.</param>
		/// <param name="intoSolution">Into solution.</param>
		Solution Mutate(Solution fromSolution, Solution intoSolution, double amplitude);
	}
	public class GeneticAlgorithm
	{
		
		public static Solution[] FindBestSolutions(
			  int runTime, 
	          int populationCount, 
	          int depth, 
	          int solutionSize,
			  ISolutionDefinition solutionDefinition,
	          Solution[] initialSolution)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var ga = new GeneticAlgorithm(depth, solutionSize, populationCount, solutionDefinition);
			ga.AddInitial(initialSolution);
			while (stopwatch.ElapsedMilliseconds < runTime)
			{
				ga.CreateNextGeneration(1 - (double)stopwatch.ElapsedMilliseconds / (double)runTime);
			}
			return ga.GetSortedResult();
		}

		public int newCreations = 0;
		private readonly int _crossOverCount;
		private readonly int _mutationCount;

		private readonly int _depth;
		private readonly int _size;
		private readonly int _populationSize;
		public static readonly Random _rnd = new Random();
		private Solution[] _solutions;
		private Solution[] _nextSolutions;
		private ISolutionDefinition _solutionDefinition;
		private Stack<Solution> _garbadeSolutions;

		private GeneticAlgorithm(int depth, 
		                         int size, 
		                         int populationSize, 
		                         ISolutionDefinition solutionDefinition)
		{
			_garbadeSolutions = new Stack<Solution>();
			_solutionDefinition = solutionDefinition;
			_depth = depth;
			_size = size;
			_populationSize = populationSize;
			_crossOverCount = (int)Math.Truncate(populationSize * 0.6);
			_mutationCount = populationSize - 2 - _crossOverCount;
			_solutions = new Solution[populationSize];
			_nextSolutions = new Solution[populationSize];

			for (int i = 0; i < _populationSize; i++)
			{
				_garbadeSolutions.Push(new Solution(_depth, _size));
			}
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
					_solutions[i] = _solutionDefinition.CreateRandom(_depth, _size);
				}
			}
			for (k = 0; k < _populationSize; k++)
			{
				_solutions[k].Evaluation = _solutionDefinition.Evaluation(_solutions[k]);
			}
		}

		private Solution[] GetSortedResult()
		{
			return _solutions.OrderByDescending(s => s.Evaluation).ToArray();
		}

		private Solution GetNew()
		{
			if (_garbadeSolutions.Count > 0)
			{
				var sol = _garbadeSolutions.Pop();
				sol.Evaluation = null;
				return sol;
			}
			newCreations++;
			return new Solution(_depth, _size);
		}

		private int k;
		private void CreateNextGeneration(double amplitude)
		{
			_nextSolutions[0] = FindBest(null);
			_nextSolutions[1] = FindBest(except:_nextSolutions[0]);
			for (int i = 0; i < _mutationCount; i++)
			{
				_nextSolutions[i + 2] = _solutionDefinition.Mutate(_solutions[_rnd.Next(0, _populationSize)], GetNew(), amplitude);
			}
			for (int i = 0; i < _crossOverCount; i++)
			{
				_nextSolutions[i + 2 + _mutationCount] = CrossOver(_solutions[_rnd.Next(0, _populationSize)], _solutions[_rnd.Next(0, _populationSize)], GetNew());   
			}
			for (int l = 0; l < _populationSize; l++)
			{
				if (!_nextSolutions.Contains(_solutions[l]))
				{
					_garbadeSolutions.Push(_solutions[l]);
				}
			}
			var temp = _solutions;
			_solutions = _nextSolutions;
			_nextSolutions = temp;
			for (k = 0; k < _populationSize; k++)
			{
				if(_solutions[k].Evaluation==null) _solutions[k].Evaluation = _solutionDefinition.Evaluation(_solutions[k]);
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

		private Solution CrossOver(Solution sol1, Solution sol2, Solution garbage)
		{
			if (sol1 == null || sol2 == null || garbage == null)
			{

			}
			if (sol1 == sol2) return sol1;
			for (int i = 0; i < _depth; i++)
			{
				for (var l = 0; l < _size; l++)
				{
					if (_rnd.Next(0, 2) == 1) garbage.Data[i, l] = sol2.Data[i, l];
					else garbage.Data[i, l] = sol1.Data[i, l];
				}
			}
			return sol1;
		}
	}

	public class Solution
	{
		public Solution(int depth, int size)
		{
			Data = new double[depth,size];
		}

		public double? Evaluation;
		public double[,] Data;
	}
}
