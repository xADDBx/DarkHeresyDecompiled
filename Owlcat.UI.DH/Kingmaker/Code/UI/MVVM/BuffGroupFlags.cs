using System;

namespace Kingmaker.Code.UI.MVVM;

[Flags]
public enum BuffGroupFlags
{
	None = 0,
	CriticalEffects = 1,
	StatusEffects = 2,
	DotEffects = 4,
	NegativeEffects = 8,
	PositiveEffects = 0x10,
	HideEmptyGroup = 0x20,
	All = 0x1F
}
