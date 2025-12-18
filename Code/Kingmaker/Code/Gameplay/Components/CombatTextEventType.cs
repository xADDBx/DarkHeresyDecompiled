using System;

namespace Kingmaker.Code.Gameplay.Components;

[Flags]
public enum CombatTextEventType
{
	None = 0,
	OnAttach = 1,
	OnDetach = 2,
	All = -1
}
