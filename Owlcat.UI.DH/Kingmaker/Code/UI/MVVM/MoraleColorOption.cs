using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class MoraleColorOption
{
	public MoraleColorOptionType Type;

	public int MinValue;

	public int MaxValue;

	public Color32 Color;

	public Color32 TextColor;

	public Sprite BackgroundSprite;

	public CanvasGroup[] ActiveGroups;

	public CanvasGroup[] DisabledGroups;
}
