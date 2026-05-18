using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconStatValueVM : TooltipBrickVM
{
	public readonly TextValueAddElement Info;

	public readonly Sprite Icon;

	public readonly Color? IconColor;

	public readonly float? IconSize;

	public readonly string IconText;

	public readonly BrickElementPalette Type;

	public readonly BrickElementPalette BackgroundType;

	public readonly bool HasValue;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly ReactiveProperty<string> m_ReactiveValue;

	private readonly ReactiveProperty<string> m_ReactiveAddValue;

	public ReadOnlyReactiveProperty<string> ReactiveValue => m_ReactiveValue;

	public ReadOnlyReactiveProperty<string> ReactiveAddValue => m_ReactiveAddValue;

	public BrickIconStatValueVM(TextValueAddElement info, Sprite icon = null, BrickElementPalette type = BrickElementPalette.Normal, BrickElementPalette backgroundType = BrickElementPalette.Normal, TooltipBaseTemplate tooltip = null, ReactiveProperty<string> reactiveValue = null, ReactiveProperty<string> reactiveAddValue = null, Color? iconColor = null, float? iconSize = null, string iconText = null, bool hasValue = true)
	{
		Info = info;
		Icon = icon;
		IconColor = iconColor;
		IconSize = iconSize;
		IconText = iconText;
		Type = type;
		BackgroundType = backgroundType;
		HasValue = hasValue;
		Tooltip = tooltip;
		m_ReactiveValue = reactiveValue;
		m_ReactiveAddValue = reactiveAddValue;
	}
}
