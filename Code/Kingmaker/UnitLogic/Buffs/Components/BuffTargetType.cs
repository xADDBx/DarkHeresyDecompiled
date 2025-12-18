using System;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Flags]
public enum BuffTargetType
{
	None = 0,
	Enemy = 1,
	Ally = 2,
	All = -1
}
