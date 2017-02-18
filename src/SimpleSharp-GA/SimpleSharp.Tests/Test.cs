using NUnit.Framework;
using System;
using SimpleSharp_GA;

namespace SimpleSharp.Tests
{
	[TestFixture()]
	public class Test
	{
		private static Random _rnd = new Random();

		[Test()]
		public void BasicTest()
		{
			var c = 0;
			var sols = GeneticAlgorithm.FindBestSolutions(100, 20, 5, 1, (arg) => arg.Data[0, 0] - arg.Data[1, 0] - arg.Data[2, 0] - arg.Data[3, 0] - arg.Data[4, 0], (arg1, arg2) =>
			{
				c++;
				for (int i = 0; i < 5; i++)
				{
					arg1.Data[i, 0] += _rnd.NextDouble() * arg2;
					if (arg1.Data[i, 0] > 1) arg1.Data[i, 0] -= 1.0;
				}
			}, new Solution[0]);

			//c == 36000
			Assert.True(sols[0].Evaluation > 0.99);
		}
	}
}
