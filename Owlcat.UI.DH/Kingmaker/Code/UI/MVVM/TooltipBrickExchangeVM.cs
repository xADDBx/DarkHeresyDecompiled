using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickExchangeVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly int Value;

	public readonly string AddValue;

	public readonly string ItemType;

	public readonly Sprite Icon;

	public readonly Color? IconColor;

	public readonly float? IconSize;

	public readonly string IconText;

	public readonly TooltipBrickIconStatValueType Type;

	public readonly TooltipBrickIconStatValueType BackgroundType;

	public readonly TooltipBrickIconStatValueStyle TextStyle;

	public readonly string ValueHint;

	public readonly bool HasValue;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly ReactiveProperty<string> m_ReactiveValue = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_ReactiveAddValue = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<string> ReactiveValue => m_ReactiveValue;

	public ReadOnlyReactiveProperty<string> ReactiveAddValue => m_ReactiveAddValue;

	public TooltipBrickExchangeVM(string name, int value, string addValue, string itemType, Sprite icon, Color? iconColor, float? iconSize, string iconText, TooltipBrickIconStatValueType type, TooltipBrickIconStatValueStyle textStyle, TooltipBrickIconStatValueType backgroundType, string valueHint, bool hasValue, TooltipBaseTemplate tooltip, ReactiveProperty<string> reactiveValue, ReactiveProperty<string> reactiveAddValue)
	{
		Name = name;
		Value = value;
		AddValue = addValue;
		ItemType = itemType;
		Icon = icon;
		IconColor = iconColor;
		IconSize = iconSize;
		IconText = iconText;
		Type = type;
		TextStyle = textStyle;
		BackgroundType = backgroundType;
		ValueHint = valueHint;
		HasValue = hasValue;
		Tooltip = tooltip;
		m_ReactiveValue = reactiveValue;
		m_ReactiveAddValue = reactiveAddValue;
	}
}
