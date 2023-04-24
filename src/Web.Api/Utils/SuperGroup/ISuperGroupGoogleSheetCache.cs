namespace Ulearn.Web.Api.Utils.SuperGroup;

public interface ISuperGroupGoogleSheetCache
{
	bool TryGet(string key, out (string groupName, string studentName)[] value);
	void Add(string key, (string groupName, string studentName)[] val);
	bool Replace(string key, (string groupName, string studentName)[] val);
	void Clear();
}