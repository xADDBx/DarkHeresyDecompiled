using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

internal enum WaaaghProfileCategory
{
	[Tooltip("Shadows for all visible dynamic lights.")]
	Shadows = 0,
	[DisplayInfo(name = "Geometry.OpaqueBase")]
	[Tooltip("Basic opaque geometry (e.g. rocks and buildings).")]
	Geometry_OpaqueBase = 1,
	[DisplayInfo(name = "Geometry.OpaqueAlphaTest")]
	[Tooltip("Alpha-tested geometry (e.g. foliage and hair).")]
	Geometry_OpaqueAlphaTest = 2,
	[DisplayInfo(name = "Geometry.OpaqueDistortion")]
	[Tooltip("Opaque geometry with distortion (e.g. water).")]
	Geometry_OpaqueDistortion = 3,
	[DisplayInfo(name = "Geometry.Transparent")]
	[Tooltip("All transparent geometry (also includes most VFX).")]
	Geometry_Transparent = 4,
	Terrain = 5,
	[Tooltip("Deferred Decals, GUI Decals, Terrain Stamping, and Terrain Blending.")]
	Decals = 6,
	[Tooltip("All visible dynamic lights, light cookies, and skybox.")]
	Lighting = 7,
	[Tooltip("Reflection probes and Screen-Space Reflections.")]
	Reflections = 8,
	AntiAliasing = 9,
	PostProcessing = 10,
	UI = 11,
	Misc = int.MaxValue
}
