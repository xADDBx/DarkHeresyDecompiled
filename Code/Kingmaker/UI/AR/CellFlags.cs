using System;

namespace Kingmaker.UI.AR;

[Flags]
public enum CellFlags : ushort
{
	CutE = 1,
	CutN = 2,
	CutW = 4,
	CutS = 8,
	QuadSplitSWNE = 0x100
}
