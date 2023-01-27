using System;

namespace Database.Repos.Groups;

[Flags]
public enum GroupQueryType
{
	SingleGroup = 0x01,
	SuperGroup = 0x02,
	All = SingleGroup | SuperGroup
}