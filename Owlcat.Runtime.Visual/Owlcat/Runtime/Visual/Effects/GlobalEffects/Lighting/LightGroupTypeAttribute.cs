using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Lighting;

public class LightGroupTypeAttribute : PropertyAttribute
{
	public enum DrawMode
	{
		Flag,
		Mask
	}

	public DrawMode Mode { get; private set; }

	public LightGroupTypeAttribute(DrawMode mode)
	{
		Mode = mode;
	}
}
