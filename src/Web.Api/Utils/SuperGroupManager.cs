using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos.Groups;
using Microsoft.Extensions.Options;
using Ulearn.Core.GoogleSheet;
using Ulearn.Web.Api.Utils.SuperGroup;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Utils;

public class SuperGroupManager
{
	private readonly GoogleApiClient client;
	private readonly IGroupsRepo groupsRepo;
	private readonly SuperGroupGoogleSheetCache cache;

	public SuperGroupManager(IOptions<WebApiConfiguration> options, IGroupsRepo groupsRepo, SuperGroupGoogleSheetCache cache)
	{
		client = new GoogleApiClient(options.Value.GoogleAccessCredentials);
		this.groupsRepo = groupsRepo;
		this.cache = cache;
	}

	private async Task<(string groupName, string studentName)[]> GetSpreadSheetGroups(string spreadsheetUrl)
	{
		var spreadSheet = await client.GetSheetByUrl(spreadsheetUrl);
		var range = spreadSheet.ReadRange("A:B");

		var unFilledRows = range
			.Select((p, index) => p.Count != 2
				? new { columns = p, rawIndex = index }
				: null)
			.Where(p => p != null)
			.ToList();
		if (unFilledRows.Count > 0)
			throw new GoogleSheetFormatException { RawsIndexes = unFilledRows.Select(r => r.rawIndex).ToList() };

		return range
			.Select(pair => (StringUtils.RemoveSpacesDuplicates(pair[1]), StringUtils.RemoveSpacesDuplicates(pair[0])))
			.ToArray();
	}

	/// <summary>
	/// Finding namesake users in groups
	/// </summary>
	/// <param name="groups"></param>
	/// <returns></returns>
	public Dictionary<string, string[]> GetGroupsByUserName((string groupName, string studentName)[] groups)
	{
		return groups
			.GroupBy(g => g.studentName)
			.Where(g => g
				.Select(d => d.groupName)
				.Distinct()
				.Count() > 1)
			.ToDictionary(
				g => g.Key,
				g => g
					.Select(d => d.groupName)
					.Distinct()
					.ToArray());
	}

	public async Task<
		(List<SingleGroup> createdGroups,
		(string groupName, string studentName)[] spreadSheetGroups)
	> GetSheetGroupsAndCreatedGroups(string spreadsheetUrl, int superGroupId, bool useCache = true)
	{
		var cached = cache.TryGet(spreadsheetUrl, out var cachedSpreadSheetGroups);
		if (useCache && cached)
			return (await groupsRepo.FindGroupsBySuperGroupIdAsync(superGroupId, true), cachedSpreadSheetGroups);

		var spreadSheetGroupsTask = GetSpreadSheetGroups(spreadsheetUrl);
		var createdGroupsTask = groupsRepo.FindGroupsBySuperGroupIdAsync(superGroupId, true);

		await Task.WhenAll(spreadSheetGroupsTask, createdGroupsTask);

		var spreadSheetGroups = await spreadSheetGroupsTask;
		var createdGroups = await createdGroupsTask;

		if (cached)
			cache.Replace(spreadsheetUrl, spreadSheetGroups);
		else
			cache.Add(spreadsheetUrl, spreadSheetGroups);

		return (createdGroups, spreadSheetGroups);
	}
}