using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconStatValueVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly string Value;

	public readonly string AddValue;

	public readonly Sprite Icon;

	public readonly Color? IconColor;

	public readonly float? IconSize;

	public readonly string IconText;

	public readonly TooltipBrickIconStatValueType Type;

	public readonly TooltipBrickIconStatValueType BackgroundType;

	public readonly string ValueHint;

	public readonly bool HasValue;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly ReactiveProperty<string> m_ReactiveValue;

	private readonly ReactiveProperty<string> m_ReactiveAddValue;

	public ReadOnlyReactiveProperty<string> ReactiveValue => m_ReactiveValue;

	public ReadOnlyReactiveProperty<string> ReactiveAddValue => m_ReactiveAddValue;

	public TooltipBrickIconStatValueVM(string name, string value, string addValue, Sprite icon, Color? iconColor, float? iconSize, string iconText, TooltipBrickIconStatValueType type, TooltipBrickIconStatValueType backgroundType, string valueHint, bool hasValue, TooltipBaseTemplate tooltip, ReactiveProperty<string> reactiveValue, ReactiveProperty<string> reactiveAddValue)
	{
		Name = name;
		Value = value;
		AddValue = addValue;
		Icon = icon;
		IconColor = iconColor;
		IconSize = iconSize;
		IconText = iconText;
		Type = type;
		BackgroundType = backgroundType;
		ValueHint = valueHint;
		HasValue = hasValue;
		Tooltip = tooltip;
		m_ReactiveValue = reactiveValue;
		m_ReactiveAddValue = reactiveAddValue;
	}
}
