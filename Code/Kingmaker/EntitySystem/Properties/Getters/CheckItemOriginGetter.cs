using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("c85b9c291fd7b824ea1191f9d887605a")]
public class CheckItemOriginGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility
{
	public ItemsItemOrigin Origin;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check origin of item is {Origin}";
	}

	protected override int GetBaseValue()
	{
		if (GetItem()?.Blueprint.Origin != Origin)
		{
			return 0;
		}
		return 1;
	}

	[CanBeNull]
	private ItemEntity GetItem()
	{
		object obj = this.GetAbilityWeapon();
		if (obj == null)
		{
			AbilityData ability = this.GetAbility();
			if ((object)ability == null)
			{
				return null;
			}
			obj = ability.SourceItem;
		}
		return (ItemEntity)obj;
	}
}
