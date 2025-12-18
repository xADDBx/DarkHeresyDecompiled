using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects;

[Serializable]
public class GlobalEffectLifetime
{
	[Min(0f)]
	public float FadeIn;

	[Min(0f)]
	public float Main;

	[Min(0f)]
	public float FadeOut;
}
