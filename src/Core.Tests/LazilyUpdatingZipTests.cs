using System.Linq;
using NUnit.Framework;

namespace Ulearn.Core.Tests
{
	[TestFixture]
	public class LazilyUpdatingZipTests
	{
		[Test]
		public void TestGetDirectoriesList()
		{
			const string fileName = "directory-1/directory-2/subdirectory/file.txt";
			var directories = LazilyUpdatingZip.GetDirectoriesList(fileName).ToList();
			CollectionAssert.Contains(directories, "directory-1");
			CollectionAssert.Contains(directories, "directory-2");
			CollectionAssert.Contains(directories, "subdirectory");
			CollectionAssert.DoesNotContain(directories, "file.txt");
		}
	}
}