using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("c7ad576e81e4478a83e2cb2a3814a49f")]
public class HasBuffsFromGroupGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public bool OnlyFromEntity;

	[ShowIf("OnlyFromEntity")]
	public PropertyTargetType Target;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is UnitEntity unitEntity))
		{
			return false;
		}
		if (OnlyFromEntity)
		{
			BaseUnitEntity caster = EvalContext.Current.GetEntityByType(Target) as BaseUnitEntity;
			return unitEntity.Buffs.Enumerable.Where((Buff buffCheck) => buffCheck.Context.MaybeCaster == caster).Any((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => Groups.Contains(group)));
		}
		return unitEntity.Buffs.Enumerable.Any((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => Groups.Contains(group)));
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = (OnlyFromEntity ? (" by " + Target.Colorized() + " ") : " ");
		return FormulaTargetScope.Current + " Has buffs" + text + "from [" + string.Join("|", from i in Groups
			where i != null
			select i.ToString()) + "]";
	}
}
