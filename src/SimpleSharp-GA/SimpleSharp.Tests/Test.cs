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
			var def = new GADefinition();
			var sols = GeneticAlgorithm.FindBestSolutions(3000, 20, 5, 1, def, new Solution[0]);

			Assert.True(def.Evaluations > 45000);
			Assert.True(sols[0].Evaluation > 0.99);
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
			return arg.Data[0, 0] - arg.Data[1, 0] - arg.Data[2, 0] - arg.Data[3, 0] - arg.Data[4, 0];
		}

		public Solution Mutate(Solution arg1, Solution intoSolution, double arg2)
		{
			Mutations++;
			for (int i = 0; i < 5; i++)
			{
				var max = arg2 + arg1.Data[i, 0];
				if (max > 1) max = 1.0;
				var min = -arg2 + arg1.Data[i, 0];
				if (min < 0) min = 0;
				intoSolution.Data[i, 0] = GeneticAlgorithm._rnd.NextDouble() * (max - min) + min;
			}
			return intoSolution;
		}

	}
}
