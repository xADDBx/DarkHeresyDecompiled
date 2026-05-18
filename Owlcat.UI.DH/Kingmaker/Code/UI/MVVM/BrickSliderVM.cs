using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSliderVM : TooltipBrickVM
{
	public readonly AutoDisposingList<SliderValuesVM> SliderValueVMs = new AutoDisposingList<SliderValuesVM>();

	public readonly int MinValue;

	public readonly int MaxValue;

	public readonly int Value;

	public readonly bool ShowValue;

	public readonly Color32 FillColor;

	public readonly string MaxValueText;

	public BrickSliderVM(int minValue, int maxValue, int value, List<SliderValuesVM> sliderValueVMs, bool showValue = false, Color fillColor = default(Color), string maxValueText = null)
	{
		SliderValueVMs.AddRange(sliderValueVMs);
		MinValue = minValue;
		MaxValue = maxValue;
		Value = value;
		ShowValue = showValue;
		FillColor = fillColor;
		MaxValueText = maxValueText;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		SliderValueVMs.Clear();
	}
}
