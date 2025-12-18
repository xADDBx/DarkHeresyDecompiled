using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5fb4a7ae99fd47f99ef012e9f4700ba0")]
public class CheckIsDOTGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalRule, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalAbility
{
	[FormerlySerializedAs("ByType")]
	public bool CheckType;

	[ShowIf("CheckType")]
	public DOT Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!CheckType)
		{
			return "Is DOT";
		}
		return $"Is {Type} DOT";
	}

	protected override bool GetBaseValue()
	{
		IEnumerable<BlueprintComponent> enumerable = this.GetRule()?.Reason.Fact?.Blueprint.ComponentsArray;
		object obj = enumerable;
		if (obj == null)
		{
			enumerable = this.GetMechanicContext()?.Blueprint.ComponentsArray;
			obj = enumerable ?? this.GetAbility()?.Blueprint.ComponentsArray;
		}
		return ((IEnumerable<BlueprintComponent>)obj)?.Any((BlueprintComponent c) => c is DOTLogic dOTLogic && (!CheckType || dOTLogic.Type == Type)) ?? false;
	}
}
