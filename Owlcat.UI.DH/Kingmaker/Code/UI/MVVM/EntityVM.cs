using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityVM : ViewModel
{
	private readonly Action m_EntityIconClick;

	private readonly Action m_UnitNameClick;

	public Sprite Icon { get; private set; }

	public string Name { get; private set; }

	public TooltipBaseTemplate Tooltip { get; private set; }

	public EntityVM(BaseUnitEntity unit, ItemEntity item)
	{
		Icon = item?.Icon;
		Name = unit?.CharacterName;
		Tooltip = new TooltipTemplateItem(item);
		m_EntityIconClick = delegate
		{
		};
		m_UnitNameClick = delegate
		{
			UIUtilityCamera.ShowUnit(unit);
		};
	}

	public EntityVM(BaseUnitEntity unit, AbilityData ability)
	{
		Icon = ability?.Icon;
		Name = unit?.CharacterName;
		Tooltip = new TooltipTemplateAbility(ability);
		m_EntityIconClick = delegate
		{
		};
		m_UnitNameClick = delegate
		{
			UIUtilityCamera.ShowUnit(unit);
		};
	}

	public void OnEntityIconClick()
	{
		m_EntityIconClick();
	}

	public void OnUnitNameClick()
	{
		m_UnitNameClick();
	}
}
