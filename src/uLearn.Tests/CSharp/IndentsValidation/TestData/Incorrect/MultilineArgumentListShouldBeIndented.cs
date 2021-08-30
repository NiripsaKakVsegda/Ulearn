using System;
using System.Linq;

namespace Incorrect
{
	public class MultilineArgumentListShouldBeIndented
	{
		// ReSharper disable once FunctionRecursiveOnAllPaths
		public static void Main(string s1, string s2, string s3)
		{
			new[] { 1, 2, 3 }.Select(
i => i);

			new[] { 1, 2, 3 }.Select(
	i => i);

			new[] { 1, 2, 3 }.Select(
		i => i);

			new[] { 1, 2, 3 }.Select(
			i => i);

			new[] { 1, 2, 3 }.Select(
i => new[] {i}
.Select(a => a));

			new[] { 1, 2, 3 }.Select(
i => new[] {i}
	.Select(a => a));

			new[] { 1, 2, 3 }.Select(
i => new[] {i}
		.Select(a => a));

			new[] { 1, 2, 3 }.Select(
i => new[] {i}
			.Select(a => a));

			new[] { 1, 2, 3 }.Select(
i => new[] {i}
				.Select(a => a));

			new[] { 1, 2, 3 }.Select(
				i => new[] { i }
.Select(a => a));

			new[] { 1, 2, 3 }.Select(
				i => new[] { i }
	.Select(a => a));

			new[] { 1, 2, 3 }.Select(
				i => new[] { i }
		.Select(a => a));

			new[] { 1, 2, 3 }.Select(
				i => new[] { i }
			.Select(a => a));

			Main(
			s1, 
				s2, 
				s3);


			Main(
				s1,
					s2,
					s3);

			Main(
				s1,
					s2,
						s3);

			Main(
					s1,
					s2,
						s3);

			Main(
					s1,
						s2,
							s3);

			Main(
			s1, 
			s2, 
				s3);

			Main(
			s1, 
			s2, 
			s3);

			Main(
		s1,
			s2,
				s3);

			Main(
s1,
s2,
s3);

			Main(
		s1,
		s2,
		s3);

			Main(
			s1,
			s2,
			s3);

			M((i1, i2) => i1.Select(
			i => i).ToArray());

			M((i1, i2) => i1.Select(
i => i).ToArray());

			M((i1, i2) => i1.Select(
		i => i).ToArray());

			M(
			(i1, i2) => i1.Select(i => i).ToArray());

			M(
		(i1, i2) => i1.Select(i => i).ToArray());

			M(
(i1, i2) => i1.Select(i => i).ToArray());

			M(
(i1, i2) => i1.Select(
	i => i).ToArray());

			M(
(i1, i2) => i1.Select(
i => i).ToArray());

			M(
				(i1, i2) => i1.Select(
				i => i).ToArray());

			M(
				(i1, i2) => i1.Select(
			i => i).ToArray());
		}

		public static void M(Func<int[], int[], int[]> func)
		{
			
		}
	}
}