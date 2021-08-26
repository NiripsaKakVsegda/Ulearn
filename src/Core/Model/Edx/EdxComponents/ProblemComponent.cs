using System.IO;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	// Этот компонент не создается из курса.
	// Компонент реализован так, чтобы созданыне вручную не удалялись из курса и соранялм содержимое неизменным.
	public class ProblemComponent : Component
	{
		public string FileContent;

		private const string subfolderName = "problem";
		public override string SubfolderName => subfolderName;

		public override EdxReference GetReference()
		{
			return new ProblemComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return FileContent;
		}

		public static ProblemComponent Load(string folderName, string urlName, EdxLoadOptions options)
		{
			var problem = new ProblemComponent
			{
				FileContent = File.ReadAllText(Path.Combine(folderName, subfolderName, $"{urlName}.xml"))
			};
			return Load(folderName, subfolderName, urlName, options, null, () => problem);
		}

		public override void Save(string folderName)
		{
			var path = Path.Combine(folderName, SubfolderName);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			var filename = Path.Combine(path, UrlName + ".xml");
			File.WriteAllText(filename, AsHtmlString());
		}
	}
}