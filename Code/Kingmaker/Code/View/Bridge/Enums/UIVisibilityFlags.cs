using System;

namespace Kingmaker.Code.View.Bridge.Enums;

[Flags]
public enum UIVisibilityFlags
{
	None = 0,
	StaticPart = 1,
	DynamicPart = 2,
	CommonPart = 4,
	Pointer = 8,
	BugReport = 0x10
}
