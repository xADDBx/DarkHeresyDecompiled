using System.Collections.Generic;
using System.Text;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("23ddfb172c2d3c144ab007dec380d712")]
public class ContextActionRemoveBuff : ContextAction
{
	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintBuff> _buff;

	public bool RemoveFromCaster;

	[InfoBox("Снимать бафф только от того же кастера, который выполняет экшен")]
	public bool CheckCaster;

	[InfoBox("Если true, то экшен снимает не все стаки баффа, а указанное в RanksCount количество")]
	public bool RemoveRanks;

	[ShowIf("RemoveRanks")]
	public ContextValue RanksCount = new ContextValue
	{
		Value = 1
	};

	public BlueprintBuff Buff => _buff;

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Remove ");
		if (RemoveRanks)
		{
			builder.Append(RanksCount);
			builder.Append(" ranks of buff ");
		}
		else
		{
			builder.Append("buff ");
		}
		builder.Append(_buff);
		if (CheckCaster)
		{
			builder.Append(" (applied by same caster)");
		}
		builder.Append(" from ");
		builder.Append(RemoveFromCaster ? "caster" : "target");
		return builder.ToString();
	}

	protected override void RunAction()
	{
		MechanicEntity target = (RemoveFromCaster ? base.Caster : base.Target.Entity);
		List<Buff> value;
		using (CollectionPool<List<Kingmaker.UnitLogic.Buffs.Buff>, Kingmaker.UnitLogic.Buffs.Buff>.Get(out value))
		{
			CollectSuitableBuffs(target, value);
			using (SimpleContextData<MechanicEntity, Kingmaker.UnitLogic.Buffs.Buff.Scope.Caster>.Set(base.Caster))
			{
				int count = (RemoveRanks ? RanksCount.Calculate(base.Context) : 0);
				foreach (Buff item in value)
				{
					if (RemoveRanks)
					{
						item.RemoveRank(count, base.Caster);
					}
					else
					{
						item.Remove();
					}
				}
			}
		}
	}

	private void CollectSuitableBuffs(MechanicEntity target, List<Buff> result)
	{
		foreach (Buff rawFact in target.Buffs.RawFacts)
		{
			if (rawFact.Blueprint == Buff && (!CheckCaster || rawFact.Caster == base.Caster))
			{
				result.Add(rawFact);
			}
		}
	}
}
