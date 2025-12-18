using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public sealed class HDRACESPresetParameter : VolumeParameter<HDRACESPreset>
{
	public HDRACESPresetParameter(HDRACESPreset value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
