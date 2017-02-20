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

		private readonly int _eliteChildren = 1;
		private readonly int _crossOverCount;
		private readonly int _mutationCount;

		private readonly int _depth;
		private readonly int _size;
		private readonly int _populationSize;
		public static readonly Random _rnd = new Random();
		private Solution[] _solutions;
		private Solution[] _nextSolutions;
		private ISolutionDefinition _solutionDefinition;

		private GeneticAlgorithm(int depth, 
		                         int size, 
		                         int populationSize, 
		                         ISolutionDefinition solutionDefinition)
		{
			_solutionDefinition = solutionDefinition;
			_depth = depth;
			_size = size;
			_populationSize = populationSize;
			_crossOverCount = (int)Math.Truncate(populationSize * 0.6);
			_mutationCount = populationSize - _eliteChildren - _crossOverCount;
			_solutions = new Solution[populationSize];
			_nextSolutions = new Solution[populationSize];

			for (int i = 0; i < _populationSize; i++)
			{
				_nextSolutions[i] = new Solution(_depth, _size);
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
			for (var k = 0; k < _populationSize; k++)
			{
				_solutions[k].Evaluation = _solutionDefinition.Evaluation(_solutions[k]);
			}
		}

		private Solution[] GetSortedResult()
		{
			return _solutions.OrderByDescending(s => s.Evaluation).ToArray();
		}

		private Solution[] Next = new Solution[1];
		private void CreateNextGeneration(double amplitude)
		{
			//Elite children
			var bestPos = FindBest(null);
			for (int i = 0; i < _populationSize; i++)
			{
				if (_nextSolutions[i] == _solutions[bestPos])
				{
					_nextSolutions[i] = new Solution(_depth, _size);
				}
			}
			_nextSolutions[0] = _solutions[bestPos];

			//var secondBestPos = FindBest(except: _nextSolutions[0]);
			//for (int i = 0; i < _populationSize; i++)
			//{
			//	if (_nextSolutions[i] == _solutions[secondBestPos])
			//	{
			//		_nextSolutions[i] = new Solution(_depth, _size);
			//	}
			//}
			//_nextSolutions[1] = _solutions[secondBestPos];

			//Mutate best
			_nextSolutions[1].Evaluation = null;
			_nextSolutions[1] = _solutionDefinition.Mutate(_nextSolutions[0], _nextSolutions[1], amplitude);

			//Mutate
			for (int i = 0; i < _mutationCount-1; i++)
			{
				Next[0] = _nextSolutions[i + _eliteChildren + 1];
				Next[0].Evaluation = null;
				_nextSolutions[i + _eliteChildren+1] = _solutionDefinition.Mutate(_solutions[_rnd.Next(0, _populationSize)], Next[0], amplitude);
			}

			//CrossOver
			for (int i = 0; i < _crossOverCount-1; i++)
			{
				Next[0] = _nextSolutions[i + _eliteChildren + _mutationCount];
				Next[0].Evaluation = null;
				var f = _rnd.Next(0, _populationSize);
				var s = _rnd.Next(0, _populationSize);
				while (s == f) s = _rnd.Next(0, _populationSize);
				_nextSolutions[i + _eliteChildren + _mutationCount] = CrossOver(_solutions[f], _solutions[s], Next[0]);   
			}

			//One random.
			_nextSolutions[_populationSize - 1] = _solutionDefinition.CreateRandom(_depth,_size);

			var temp = _solutions;
			_solutions = _nextSolutions;
			_nextSolutions = temp;

			for ( var k = 0; k < _populationSize; k++)
			{
				if (_solutions[k].Evaluation == null)
				{
					_solutions[k].Evaluation = _solutionDefinition.Evaluation(_solutions[k]);
				}
				else {
					NonEvaluations++;
				}
			}
		}
		private int NonEvaluations = 0;

		private int FindBest( Solution except)
		{
			var current = double.MinValue;
			int found = 0;
			for (var k = 0; k < _populationSize; k++)
			{
				if (_solutions[k].Evaluation > current && except != _solutions[k])
				{
					current = _solutions[k].Evaluation.Value;
					found = k;
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
			for (int i = 0; i < _depth; i++)
			{
				for (var l = 0; l < _size; l++)
				{
					if (_rnd.Next(0, 2) == 1) garbage.Data[i, l] = sol2.Data[_rnd.Next(0,_depth), l];
					else garbage.Data[i, l] = sol1.Data[_rnd.Next(0,_depth), l];
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

		public double? Evaluation = null;
		public double[,] Data;
	}
}
