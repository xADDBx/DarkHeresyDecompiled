using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("84878892b53b44cbadb80f6d8332b5cd")]
public abstract class BuffTrigger : MechanicEntityFactComponentDelegate
{
	protected enum EventType
	{
		Added,
		Removed,
		RankIncreased,
		RankDecreased
	}

	public enum BuffFilterType
	{
		Any,
		SingleBuff,
		BuffList,
		SingleGroup,
		GroupList
	}

	public enum CasterType
	{
		Any,
		Owner,
		Same
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public CasterType CasterFilter;

	public BuffFilterType BuffFilter;

	[ShowIf("IsFilterSingleBuff")]
	public BpRef<BlueprintBuff> FilterSingleBuff;

	[ShowIf("IsFilterBuffList")]
	public BpRef<BlueprintBuff>[] FilterBuffList;

	[ShowIf("IsFilterSingleGroup")]
	public BpRef<BlueprintAbilityGroup> FilterSingleGroup;

	[ShowIf("IsFilterGroupList")]
	public BpRef<BlueprintAbilityGroup>[] FilterGroupList;

	[InfoBox("Выполнять RankIncreased при добавлении и RankDecreased при удалении баффа")]
	public bool TreatAddAndRemoveAsRankChange;

	[InfoBox("Включает возможность наложить Child Buff (ContextActionApplyBuff) родителем которого будет бафф, стриггеривший этот компонент.")]
	public bool ParentIsTriggeringBuff;

	[InfoBox("Меняет цель экшенов на кастера выдавшего/забравшего бафф или изменившего его ранг. По умолчанию целью экшенов является тот, на кого наложен бафф, стриггеривший этот компонент.")]
	public bool TargetCaster;

	[HideIf("TreatAddAndRemoveAsRankChange")]
	public ActionList Added = new ActionList();

	[HideIf("TreatAddAndRemoveAsRankChange")]
	public ActionList Removed = new ActionList();

	public ActionList RankIncreased = new ActionList();

	public ActionList RankDecreased = new ActionList();

	private bool IsFilterSingleBuff => BuffFilter == BuffFilterType.SingleBuff;

	private bool IsFilterBuffList => BuffFilter == BuffFilterType.BuffList;

	private bool IsFilterSingleGroup => BuffFilter == BuffFilterType.SingleGroup;

	private bool IsFilterGroupList => BuffFilter == BuffFilterType.GroupList;

	protected void HandleEvent(EventType e, Buff buff, MechanicEntity caster)
	{
		HandleEvent(e, buff, buff.Rank, caster);
	}

	protected void HandleEvent(EventType e, Buff buff, int delta, MechanicEntity caster)
	{
		if (!IsSuitable(buff, caster))
		{
			return;
		}
		delta = ((e == EventType.Removed || e == EventType.RankDecreased) ? (-delta) : delta);
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(GetTarget(buff, caster)))
		{
			using (SimpleContextData<int, Buff.Scope.RankDelta>.Set(delta))
			{
				using (SimpleContextData<MechanicEntityFact, Buff.Scope.Parent>.SetIfNotNull(ParentIsTriggeringBuff ? buff : null))
				{
					switch (e)
					{
					case EventType.Added:
						if (TreatAddAndRemoveAsRankChange)
						{
							RankIncreased.Run();
						}
						else
						{
							Added.Run();
						}
						break;
					case EventType.Removed:
						if (TreatAddAndRemoveAsRankChange)
						{
							RankDecreased.Run();
						}
						else
						{
							Removed.Run();
						}
						break;
					case EventType.RankIncreased:
						RankIncreased.Run();
						break;
					case EventType.RankDecreased:
						RankDecreased.Run();
						break;
					default:
						throw new ArgumentOutOfRangeException("e", e, null);
					}
				}
			}
		}
	}

	private MechanicEntity GetTarget(Buff buff, MechanicEntity caster)
	{
		if (!TargetCaster)
		{
			return buff.Owner;
		}
		return caster;
	}

	private bool IsSuitable(Buff buff, MechanicEntity caster)
	{
		if (BuffFilter switch
		{
			BuffFilterType.Any => 1, 
			BuffFilterType.SingleBuff => (FilterSingleBuff == buff.Blueprint) ? 1 : 0, 
			BuffFilterType.BuffList => FilterBuffList.Contains(buff.Blueprint) ? 1 : 0, 
			BuffFilterType.SingleGroup => buff.Blueprint.AbilityGroups.Contains(FilterSingleGroup) ? 1 : 0, 
			BuffFilterType.GroupList => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup i) => FilterGroupList.Contains(i)) ? 1 : 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		} == 0)
		{
			return false;
		}
		if (CasterFilter switch
		{
			CasterType.Any => 1, 
			CasterType.Owner => (caster == base.Owner) ? 1 : 0, 
			CasterType.Same => (caster == base.Context.MaybeCaster) ? 1 : 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		} == 0)
		{
			return false;
		}
		return Restrictions.IsPassed(buff.Context, base.Owner, GetTarget(buff, caster));
	}
}
