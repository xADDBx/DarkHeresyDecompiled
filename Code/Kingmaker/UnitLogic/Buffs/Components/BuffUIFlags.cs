using System;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Flags]
public enum BuffUIFlags
{
	None = 0,
	LightStatusEffect = 1,
	ModerateStatusEffect = 2,
	SevereStatusEffect = 4
}
