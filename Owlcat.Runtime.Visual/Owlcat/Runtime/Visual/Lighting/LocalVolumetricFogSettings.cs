using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

[Serializable]
public class LocalVolumetricFogSettings
{
	public LocalVolumetricFogResolution MaxLocalVolumetricFogSize = LocalVolumetricFogResolution.Resolution32;

	[Range(1f, 512f)]
	public int MaxLocalVolumetricFogOnScreen = 64;

	[Range(1f, 64f)]
	public int MaxTexturesInAtlas = 4;
}
