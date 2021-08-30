using System.Linq;

namespace Correct
{
	public class MultilineExpressionStatementIsIndented
	{
		public static void Main(string[] args)
		{
			new[] { 1, 2, 3 }
				.Select(i => i);

			new[] { 1, 2, 3 }
					.Select(i => i);

			new[] { 1, 2, 3 }
				.Select(i => i)
				.Where(i => i < 2)
				.ToArray();

			new[] { 1, 2, 3 }
				.Select(i => i)
					.Where(i => i < 2)
					.ToArray();

			new[] { 1, 2, 3 }
				.Select(i => i)
						.Where(i => i < 2)
						.ToArray();

			new[] { 1, 2, 3 }
				.Select(i => i)
					.Where(i => i < 2)
					.ToArray();

			new[] { 1, 2, 3 }
				.Select(i => i)
						.Where(i => i < 2)
							.ToArray();

			new[] { 1, 2, 3 }.Select(i => i)
				.Where(i => i < 2)
				.ToArray();

			new[] { 1, 2, 3 }.Select(i => i)
					.Where(i => i < 2)
					.ToArray();

			new[] { 1, 2, 3 }.Select(i => i)
				.Where(i => i < 2)
					.ToArray();

			new[] { 1, 2, 3 }
					.Select(i => new[] { i }
						.Select(i1 => i1));

			new[] { 1, 2, 3 }
						.Select(i => new[] { i }
							.Select(i1 => i1));

			new[] { 1, 2, 3 }
					.Select(i => new[] { i }
							.Select(i1 => i1));

			var a = 0; new[] { 1, 2, 3 }
				.Select(i => new[] { i });

			var e = 0; new[] { 1, 2, 3 }
					.Select(i => new[] { i });

			var j = 0; new[] { 1, 2, 3 }
					.Select(i => new[] { i })
					.Where(k => k.Length < 2);

			var j1 = 0; new[] { 1, 2, 3 }
					.Select(i => new[] { i })
						.Where(k => k.Length < 2);

			var b = 0; new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l));

			var c = 0; new[] { 1, 2, 3 }
				.Select(i => new[] { i }
						.Select(l => l));

			var d = 0; new[] { 1, 2, 3 }
				.Select(i => new[] { i }
					.Select(l => l));
			
			// todo ???
			#region ???

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

			#endregion
		}
	}
}