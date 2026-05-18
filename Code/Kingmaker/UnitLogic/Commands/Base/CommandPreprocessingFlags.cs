using System;

namespace Kingmaker.UnitLogic.Commands.Base;

[Flags]
public enum CommandPreprocessingFlags
{
	None = 0,
	InterruptCurrentAsSoonAsPossible = 1,
	ClearQueue = 2,
	SoftInterruptAll = 3
}
