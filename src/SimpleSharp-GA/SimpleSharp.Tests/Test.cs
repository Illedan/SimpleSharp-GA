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
			var sols = GeneticAlgorithm.FindBestSolutions(150, 20, 5, 1, (arg) => arg.Data[0, 0] - arg.Data[1, 0] - arg.Data[2, 0] - arg.Data[3, 0] - arg.Data[4, 0], (arg1, arg2) =>
			{

				c++;
				for (int i = 0; i < 5; i++)
				{
					var max = arg2+arg1.Data[i, 0];
					if (max > 1) max = 1.0;
					var min = -arg2 +arg1.Data[i, 0];
					if (min < 0) min = 0;
					arg1.Data[i, 0] = _rnd.NextDouble()*(max-min)+min;
				}
			}, new Solution[0]);

			Assert.True(c > 45000);
			Assert.True(sols[0].Evaluation > 0.99);
		}
	}
}
