using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class BuffCollection : MechanicEntityFactsCollection<Buff>
{
	private bool m_Disabled;

	public AbstractUnitEntity Owner => base.Manager?.Owner as AbstractUnitEntity;

	public IEnumerator<Buff> GetEnumerator()
	{
		return base.Enumerable.GetEnumerator();
	}

	[CanBeNull]
	public Buff Add(BlueprintBuff blueprint, MechanicsContext parentContext, BuffDuration duration = default(BuffDuration), int rank = 1)
	{
		return Add(blueprint, parentContext?.MaybeCaster, parentContext, duration, rank);
	}

	[CanBeNull]
	public Buff Add([CanBeNull] BlueprintBuff blueprint, [CanBeNull] MechanicEntity caster = null, MechanicsContext parentContext = null, BuffDuration duration = default(BuffDuration), int rank = 1)
	{
		if (blueprint == null)
		{
			return null;
		}
		if (Owner is LightweightUnitEntity lightweightUnitEntity)
		{
			lightweightUnitEntity.PlayBuffFx(blueprint);
			return null;
		}
		if (caster == null)
		{
			caster = parentContext?.MaybeCaster ?? Owner;
		}
		if (parentContext == null || (caster != null && caster != parentContext.MaybeCaster))
		{
			parentContext = MechanicsContext.Claim(parentContext?.Blueprint ?? blueprint, caster, parentContext?.MaybeOwner ?? Owner, parentContext, parentContext?.ClickedTarget, parentContext?.Fact);
		}
		using (SimpleContextData<MechanicEntity, Buff.Scope.Caster>.SetIfNotNull(caster))
		{
			return base.Manager.Add(new Buff(blueprint, parentContext, duration, rank));
		}
	}

	public void Remove(BlueprintBuff blueprint, MechanicEntity caster = null)
	{
		using (SimpleContextData<MechanicEntity, Buff.Scope.Caster>.SetIfNotNull(caster))
		{
			Remove((BlueprintFact)blueprint);
		}
	}

	protected override Buff PrepareFactForAttach(Buff fact)
	{
		if (m_Disabled)
		{
			return null;
		}
		BlueprintBuff blueprint = fact.Blueprint;
		MechanicEntity caster = fact.MaybeParentContext?.MaybeCaster ?? Owner;
		using MechanicsContext mechanicsContext = MechanicsContext.Claim(blueprint, caster, null, fact.MaybeParentContext);
		RuleCalculateCanApplyBuff ruleCalculateCanApplyBuff = Rulebook.Trigger(new RuleCalculateCanApplyBuff(Owner, mechanicsContext, fact));
		if (!ruleCalculateCanApplyBuff.CanApply)
		{
			return null;
		}
		BuffDuration duration = ruleCalculateCanApplyBuff.Duration;
		if (blueprint.Stacking != StackingType.Stack)
		{
			Buff buff = GetBuff(blueprint);
			if (buff != null)
			{
				switch (blueprint.Stacking)
				{
				case StackingType.Replace:
					base.Manager.Remove(buff);
					break;
				case StackingType.Prolong:
					buff.Prolong(duration.Rounds);
					return buff;
				case StackingType.Ignore:
					return buff;
				case StackingType.Summ:
					buff.IncreaseDuration(duration.Rounds);
					return buff;
				case StackingType.Rank:
					buff.AddRank(fact.Rank, caster);
					if (buff.Blueprint.ProlongWhenRankAdded)
					{
						buff.Prolong(duration.Rounds);
					}
					return buff;
				case StackingType.HighestByProperty:
					if (mechanicsContext[blueprint.PriorityProperty] >= buff.Context[blueprint.PriorityProperty])
					{
						base.Manager.Remove(buff);
						break;
					}
					return buff;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		fact.SetDuration(duration);
		return fact;
	}

	protected override Buff PrepareFactForDetach(Buff fact)
	{
		return fact;
	}

	protected override void OnFactDidAttach(Buff fact)
	{
		base.OnFactDidAttach(fact);
		MechanicEntity caster = SimpleContextData<MechanicEntity, Buff.Scope.Caster>.Current ?? fact.Caster;
		EventBus.RaiseEvent((IBaseUnitEntity)fact.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
		{
			h.HandleBuffDidAdded(fact, caster);
		}, isCheckRuntime: true);
	}

	protected override void OnFactWillDetach(Buff fact)
	{
		base.OnFactWillDetach(fact);
		MechanicEntity caster = SimpleContextData<MechanicEntity, Buff.Scope.Caster>.Current ?? fact.Caster;
		EventBus.RaiseEvent((IBaseUnitEntity)fact.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
		{
			h.HandleBuffDidRemoved(fact, caster);
		}, isCheckRuntime: true);
		fact.OnRemove();
	}

	[CanBeNull]
	public Buff GetBuff(BlueprintBuff blueprint)
	{
		foreach (Buff item in base.Enumerable)
		{
			if (item.Blueprint == blueprint)
			{
				return item;
			}
		}
		return null;
	}

	public int GetRank(BlueprintBuff blueprint)
	{
		return GetBuff(blueprint)?.Rank ?? 0;
	}

	public void SetupPreview(BaseUnitEntity owner)
	{
		m_Disabled = true;
		foreach (Buff item in base.RawFacts.ToTempList())
		{
			item.Deactivate();
		}
	}

	public void SpawnBuffsFxs()
	{
		foreach (Buff rawFact in base.RawFacts)
		{
			rawFact.SpawnParticleEffect();
		}
	}

	public void OnCombatEnded()
	{
	}
}
