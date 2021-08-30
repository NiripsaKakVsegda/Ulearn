using System;
using System.Linq;
using System.Runtime.Serialization;
using AntiPlagiarism.Web.Database;
using Newtonsoft.Json;

namespace ManualUtils.AntiPlagiarism
{
	[DataContract]
	public class AdditionalInfo
	{
		[DataMember(Name = "SubmissionId")]
		public int SubmissionId { get; set; }
	}

	public static class FillAntiplagFields
	{
		public static void FillClientSubmissionId(AntiPlagiarismDb adb)
		{
			var ids = adb.Submissions.Where(s => s.ClientSubmissionId == null).Select(s => s.Id).ToList();
			adb.DisableAutoDetectChanges();
			foreach (var id in ids)
			{
				var submission = adb.Submissions.Find(id);
				if (submission == null)
				{
					Console.WriteLine($"Stop on id {id}");
					return;
				}
				try
				{
					submission.ClientSubmissionId = JsonConvert.DeserializeObject<AdditionalInfo>(submission.AdditionalInfo).SubmissionId.ToString();
					adb.Submissions.Update(submission);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error on id {id} \"{submission.AdditionalInfo}\"");
				}
				if (id % 1000 == 0)
				{
					adb.SaveChanges();
					Console.WriteLine(id);
				}
			}
			adb.SaveChanges();
			adb.EnableAutoDetectChanges();
		}
	}
}