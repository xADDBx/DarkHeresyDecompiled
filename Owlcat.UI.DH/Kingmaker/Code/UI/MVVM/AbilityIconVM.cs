using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityIconVM : ViewModel
{
	public readonly string Acronym = string.Empty;

	public readonly Sprite Icon;

	public readonly Color IconColor = Color.white;

	public AbilityIconVM(BlueprintAbility blueprint)
		: this(blueprint.Icon, blueprint.Name)
	{
	}

	public AbilityIconVM(BlueprintFeature blueprint)
		: this(blueprint.Icon, blueprint.Name)
	{
	}

	private AbilityIconVM(Sprite icon, string name)
	{
		Icon = icon;
		if (!(Icon != null))
		{
			IconColor = UIUtilityText.GetColorByText(name);
			Icon = UIUtilityText.GetIconByText(name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(name);
		}
	}
}
