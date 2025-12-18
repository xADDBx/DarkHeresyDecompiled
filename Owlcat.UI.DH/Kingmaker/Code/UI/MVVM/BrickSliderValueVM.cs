using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSliderValueVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly int Value;

	public readonly int MinValue;

	public readonly int MaxValue;

	public readonly string Text;

	public readonly Sprite Sprite;

	public readonly bool IsValueOnBottom;

	public readonly bool NeedValueText;

	public readonly bool NeedColor;

	public readonly Color32? ValueColor;

	public readonly Color32? TextColor;

	public readonly Color32 BgrColor;

	public BrickSliderValueVM(int minValue, int maxValue, int value, Sprite sprite = null, bool needColor = false, Color32 bgrColor = default(Color32), Color32? valueColor = null, bool isValueOnBottom = true, bool needValueText = true, string text = null, Color32? textColor = null)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		Value = value;
		Sprite = sprite;
		NeedColor = needColor;
		BgrColor = bgrColor;
		ValueColor = valueColor;
		IsValueOnBottom = isValueOnBottom;
		NeedValueText = needValueText;
		Text = text;
		TextColor = textColor;
	}

	protected override void DisposeImplementation()
	{
	}
}
