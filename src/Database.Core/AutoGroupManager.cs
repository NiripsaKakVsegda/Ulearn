using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Groups;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class AutoGroupManager
{
	private readonly IGroupsRepo groupsRepo;

	public AutoGroupManager(IGroupsRepo groupsRepo)
	{
		this.groupsRepo = groupsRepo;
	}

	public async Task<(string group, string student)[]> GetRemoteDataAsync(string tableLink)
	{
		using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30)};
		var result = await client.GetStringAsync(tableLink).ConfigureAwait(false);
		var content = ParseContent(result);
		var remoteData = BuildRemoteData(content);
		
		return remoteData;
	}

	public async Task<string[]> GetLocalDataAsync(int superGroupId)
	{
		var superGroup = await groupsRepo.FindGroupByIdAsync(superGroupId) as SuperGroup;
		if (superGroup == null)
			return Array.Empty<string>();
		
		return await groupsRepo.GetCourseGroupsQueryable(superGroup.CourseId, GroupQueryType.SingleGroup)
			.Where(x => ((SingleGroup)x).SuperGroupId == superGroupId)
			.Select(x => x.Name)
			.ToArrayAsync();
	}

	private string ParseContent(string text)
	{
		var begin = "\"Лист1";
		var beginIdx = text.IndexOf(begin);
		var contextIdx = beginIdx + begin.Length + 1;
		var contentLength = 0;
		while (text[contextIdx + contentLength] != '"') contentLength++;
		return text.Substring(contextIdx, contentLength);
		//return contentEx.Match(text).Groups["content"].Value.ReplaceLineEndings("");
	}

	private (string group, string student)[] BuildRemoteData(string content)
	{
		return content.Split('\n')
			.Where(x => !string.IsNullOrEmpty(x))
			.Skip(1)
			.Select(line => (line.Split(',')[0], line.Split(',')[1]))
			.ToArray();
	}
}