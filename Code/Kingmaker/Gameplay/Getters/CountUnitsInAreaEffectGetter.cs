using System;
using System.Linq;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("5f15f5969ec54cdd957c1548ebfb951c")]
public sealed class CountUnitsInAreaEffectGetter : IntPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public bool UseAreaEffectFromContext = true;

	[HideIf("UseAreaEffectFromContext")]
	public BpRef<BlueprintAreaEffect> AreaEffect;

	[HideIf("UseAreaEffectFromContext")]
	public bool OnlySpawnedByContextOwner;

	public RestrictionCalculator UnitRestriction;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return string.Format("Count units in area effect {0}", UseAreaEffectFromContext ? ((object)"") : ((object)AreaEffect));
	}

	protected override int GetBaseValue()
	{
		return (UseAreaEffectFromContext ? SimpleContextData<AreaEffectEntity, MechanicsContext.Scope.AreaEffect>.Current : ((!OnlySpawnedByContextOwner) ? Game.Instance.EntityPools.AreaEffects.FirstOrDefault((AreaEffectEntity i) => i.Blueprint == AreaEffect.Blueprint) : this.GetMechanicContext().MaybeOwner?.GetOptional<UnitPartSpawnedAreaEffects>()?.Get(AreaEffect.Blueprint)))?.InGameEntitiesInside.Count((MechanicEntity i) => UnitRestriction.IsPassed(this.GetMechanicContext(), base.CurrentEntity, i)) ?? (-1);
	}
}
