namespace Correct
{
	public class BlockChildrenIndented
	{
		public BlockChildrenIndented()
		{
			var intendedVar = 0; var a = 0; var b = 0;
			var c = 0; var e = 0; var f = 0;
			int i = 0;
			foreach (var j in new[] { 0, 1, 2 })
			{
				intendedVar++;
			}
			foreach (var j in new[] { 0, 1, 2 })
			{
				intendedVar++;
				intendedVar++;
				intendedVar++;
				intendedVar++;
			}
			for (i = 0; i < 5; i++)
			{
				i++;
			}
			while (++i < 5)
			{
				intendedVar = 1;
			}
			do
			{
				intendedVar = 1;
			} while (i++ < 10);
			foreach (var j in new[] { 0, 1, 2 })
			{
				foreach (var k in new[] {3, 4, 5})
				{
					while (i++ < 10)
					{
						intendedVar++;
					}
				}
			}
		}
	}
}