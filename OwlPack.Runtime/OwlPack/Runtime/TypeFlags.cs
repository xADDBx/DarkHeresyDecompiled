using System;

namespace OwlPack.Runtime;

[Flags]
public enum TypeFlags : byte
{
	None = 0,
	IsExternal = 1
}
