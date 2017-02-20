using NUnit.Framework;
using System;
using SimpleSharp_GA;

namespace SimpleSharp.Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void BasicTest()
		{
			var most = 0; // 114149
			var min = 10000000; // 91109
			double bestEval = 0; 		 // 48,5493
			double lowestEval = 1000000; // 48,5361
			for (int i = 0; i < 100; i++)
			{

				var def = new GADefinition();
				var sols = GeneticAlgorithm.FindBestSolutions(150, 20, 5, 1, def, new Solution[0]);

				Assert.True(def.Evaluations > 80000);
				if (def.Evaluations > most) most = def.Evaluations;
				if (def.Evaluations < min) min = def.Evaluations;
				
				Assert.True(sols[0].Evaluation > 47.0);
				if (sols[0].Evaluation > bestEval) bestEval = sols[0].Evaluation.Value;
				if (sols[0].Evaluation < lowestEval) lowestEval = sols[0].Evaluation.Value;
			}
		}
	}

	public class GADefinition : ISolutionDefinition
	{
		public int Evaluations = 0;
		public int Mutations = 0;

		public Solution CreateRandom(int depth, int solutionSize)
		{
			var solution = new Solution(depth, solutionSize);
			for (int i = 0; i < depth; i++)
			{
				for (int l = 0; l < solutionSize; l++)
				{
					solution.Data[i, l] = GeneticAlgorithm._rnd.NextDouble();
				}
			}
			return solution;
		}

		public double Evaluation(Solution arg)
		{
			Evaluations++;
			var res = arg.Data[0, 0] - arg.Data[1, 0] - arg.Data[2, 0] - arg.Data[3, 0] - arg.Data[4, 0];
			for (int l = 0; l < arg.Data.GetLength(0); l++)
			{
				if (Math.Abs(arg.Data[l, 0] - 0.5) < 0.01)
				{
					res += 10;
				}
			}
			return res;
		}

		public Solution Mutate(Solution solToMutate, Solution intoSolution, double arg2)
		{
			Mutations++;
			for (int i = 0; i < 5; i++)
			{
				var max = arg2 + solToMutate.Data[i, 0];
				if (max > 1.0) max = 1.0;
				var min = -arg2 + solToMutate.Data[i, 0];
				if (min < 0) min = 0;
				intoSolution.Data[i, 0] = GeneticAlgorithm._rnd.NextDouble() * (max - min) + min;
			}
			return intoSolution;
		}

	}
}
