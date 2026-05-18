using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFormattedDescriptionVM : TooltipBrickVM
{
	public readonly string Description;

	[CanBeNull]
	public readonly MechanicEntity Owner;

	[CanBeNull]
	public TMP_StyleSheet StyleSheet { get; private set; }

	[CanBeNull]
	public Color? TextColor { get; private set; }

	public BrickFormattedDescriptionVM(string description, [CanBeNull] MechanicEntity owner)
	{
		Description = description;
		Owner = owner;
	}

	public BrickFormattedDescriptionVM SetStyleSheet(TMP_StyleSheet styleSheet)
	{
		StyleSheet = styleSheet;
		return this;
	}

	public BrickFormattedDescriptionVM SetTextColor(Color? color)
	{
		TextColor = color;
		return this;
	}
}
