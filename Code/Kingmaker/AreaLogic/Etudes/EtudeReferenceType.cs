using System;

namespace Kingmaker.AreaLogic.Etudes;

[Flags]
public enum EtudeReferenceType
{
	None = 0,
	Start = 1,
	Complete = 2,
	Check = 4,
	Synchronized = 8,
	EtudeRelated = 0x10,
	Pause = 0x20,
	Resume = 0x40
}
