using System;

namespace Owlcat.Runtime.Visual.Utilities;

[Flags]
public enum StencilRef
{
	Reserve0 = 1,
	SpecialPostProcessFlag = 2,
	Distortion = 4,
	OccludedObjectHighlighting = 8,
	ReceiveTerrainBlendingDecals = 0x10,
	DBufferCustomMask = 0x20,
	ShaderGraphOverride = 0x40,
	DeferredLightingOff = 0x80,
	All = 0xFF
}
