using System;
using System.Linq;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Getters.Bool;

[Serializable]
[TypeId("770c4e7231c241f7b615c5ebf90a3819")]
public class CheckSourceAbilityModifierGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalRule
{
	[SerializeField]
	private BpRef<BlueprintAbilityModifier>[] m_Modifiers;

	public BpRefArray<BlueprintAbilityModifier> Modifiers => m_Modifiers;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Source ability has modifier " + string.Join("|", from i in Modifiers
			where i != null
			select i.ToString());
	}

	protected override bool GetBaseValue()
	{
		BlueprintAbilityWrapper obj = EvalContext.Root.SourceAbility?.Blueprint;
		if (obj == null)
		{
			return false;
		}
		return obj.AllModifiers.Any((BlueprintAbilityModifier x) => Modifiers.Contains(x));
	}
}
