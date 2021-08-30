using System;
using System.Linq;

namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
	public class MultilineExpressionStatementShouldBeIndented
	{
		public static void Main(string[] args)
		{
			new[] { 1, 2, 3 }
				.Select(i => i)
			.Where(i => i < 2)
				.ToArray();

			new[] { 1, 2, 3 }
				.Select(i => new[] {i}
		.Select(A => A));

			new[] { 1, 2, 3 }
				.Select(i => new[] {i}
			.Select(A => A));

			new[] { 1, 2, 3 }
				.Select(i => new[] {i}
				.Select(A => A));

			Console.WriteLine(); new[] { 1, 2, 3 }
		.Select(i => i);

			Console.WriteLine(); new[] { 1, 2, 3 }
			.Select(i => i);

			Console.WriteLine(); new[] { 1, 2, 3 }
				.Select(i => new[] {i}
			.Select(a => a));

			Console.WriteLine(); new[] { 1, 2, 3 }
				.Select(i => new[] {i}
				.Select(a => a));

			new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l)); new[] { 1, 2, 3 }
		.Select(p => p);

			new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l)); new[] { 1, 2, 3 }
			.Select(p => p);

			new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l)); new[] { 1, 2, 3 }
			.Select(p => new[] { p }
			.Select(g => g));

			new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l)); new[] { 1, 2, 3 }
				.Select(p => new[] { p }
				.Select(g => g));

			new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l)); new[] { 1, 2, 3 }
				.Select(p => new[] { p }
			.Select(g => g));
		}
	}
}