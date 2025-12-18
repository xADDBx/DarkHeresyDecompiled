using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.BloodMask;

[Serializable]
public class BloodMaskSettings
{
	public Color BloodColor = Color.clear;

	public Texture2D BloodTexture = Texture2D.grayTexture;

	public Vector2 DefaultTileSize = Vector2.one;

	public AnimationCurve BloodFadeoutControl = AnimationCurve.Linear(0f, 0f, 1f, 0.2224f);

	[Range(0f, 1f)]
	public float HPRatio = 1f;

	[NonSerialized]
	public float UnitSizeMultiplier = 1f;

	public float FadeOut => Mathf.Clamp01(1f - BloodFadeoutControl.Evaluate(1f - HPRatio));

	public bool IsNeedUpdate { get; set; }

	public bool IsActivated { get; set; }

	public bool IsDisabled { get; set; }

	public void Reset()
	{
		HPRatio = 1f;
	}
}
