using System;

namespace Kingmaker.UI.AR;

[Flags]
public enum CombatHudCommandExecutionMode : byte
{
	Main = 1,
	PerCohesion = 2
}
