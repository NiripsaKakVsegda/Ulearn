using System;

namespace Database.Repos.Groups;

[Flags]
public enum GroupQueryType
{
	Group = 0x01,
	SuperGroup = 0x02,
	All = Group | SuperGroup
}