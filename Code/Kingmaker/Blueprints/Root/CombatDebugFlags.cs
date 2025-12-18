using System;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[Flags]
public enum CombatDebugFlags
{
	None = 0,
	All = -1,
	ShowInspectAlwaysOnHover = 1,
	ShowMapObjectTooltipTitle = 2,
	ShowNewIconAtDescription = 4
}
