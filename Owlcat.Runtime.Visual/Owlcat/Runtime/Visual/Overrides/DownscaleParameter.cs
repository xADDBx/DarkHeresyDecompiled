using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public sealed class DownscaleParameter : VolumeParameter<BloomDownscaleMode>
{
	public DownscaleParameter(BloomDownscaleMode value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
