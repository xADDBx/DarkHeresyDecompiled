using System;

namespace Kingmaker.UI.AR;

[Flags]
public enum IntermediateCellFlags : byte
{
	ConnectionS = 1,
	ConnectionE = 2,
	ConnectionN = 4,
	ConnectionW = 8
}
