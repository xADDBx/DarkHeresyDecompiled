using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects;

public class GlobalEffectContext
{
	public GlobalEffect GlobalEffect { get; internal set; }

	public Camera Camera { get; internal set; }

	public VolumeStack VolumeStack { get; internal set; }
}
