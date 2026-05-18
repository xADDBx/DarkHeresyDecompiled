using System;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[Flags]
public enum BatchLayerFlagBits : uint
{
	NonBrg = 1u,
	Default = 2u,
	DepthOnly = 4u,
	MotionVectors = 8u,
	Debug = 0x80u
}
