using System;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Gameplay.Features.Scaling.Components;

[Serializable]
public class AbilityPropertyUISettings
{
	public bool Enabled;

	[ShowIf("Enabled")]
	public UIPropertyName NameType;

	[ShowIf("NameIsCustom")]
	public LocalizedString CustomName;

	[ShowIf("Enabled")]
	public LocalizedString Description;

	[ShowIf("Enabled")]
	public string LinkKey;

	[ShowIf("Enabled")]
	public BlueprintMechanicEntityFact.Reference DescriptionFact;

	private bool NameIsCustom
	{
		get
		{
			if (Enabled)
			{
				return NameType == UIPropertyName.Custom;
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (!NameIsCustom)
			{
				return NameType.GetLocalizedName();
			}
			return CustomName;
		}
	}
}
