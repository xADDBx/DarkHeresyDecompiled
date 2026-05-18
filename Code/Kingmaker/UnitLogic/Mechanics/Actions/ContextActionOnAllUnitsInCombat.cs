using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("7b1d3a11c0f4426a8584738997ebc207")]
public class ContextActionOnAllUnitsInCombat : ContextAction
{
	public bool OnlyEnemies = true;

	[HideIf("OnlyEnemies")]
	public bool OnlyAllies;

	public bool OnlyParty;

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterNoFacts = new BlueprintUnitFactReference[0];

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterHaveAnyFact = new BlueprintUnitFactReference[0];

	public ActionList Actions;

	public bool ActionsOnRandomTarget;

	public bool NotCaster;

	public bool OnlyVisible;

	public bool OnlyNotVisible;

	public bool IncludeDead;

	public ReferenceArrayProxy<BlueprintUnitFact> FilterNoFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterNoFacts = m_FilterNoFacts;
			return filterNoFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> FilterHaveAnyFact
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterHaveAnyFact = m_FilterHaveAnyFact;
			return filterHaveAnyFact;
		}
	}

	public override string GetCaption()
	{
		return "Run a context action on all units in combat";
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.Caster;
		if (caster == null || caster is BaseUnitEntity { IsPreviewUnit: not false })
		{
			return;
		}
		List<BaseUnitEntity> list = ((!OnlyParty) ? Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && (IncludeDead || !p.LifeState.IsDead) && p.IsInCombat).ToList() : Game.Instance.Player.Party.ToList());
		if (OnlyEnemies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsEnemy(base.Context.Caster));
		}
		if (OnlyAllies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsAlly(base.Context.Caster));
		}
		if (OnlyVisible)
		{
			list.RemoveAll((BaseUnitEntity p) => LosCalculations.GetWarhammerLos(caster, p).CoverType == LosCalculations.CoverType.LosBlocker);
		}
		if (OnlyNotVisible)
		{
			list.RemoveAll((BaseUnitEntity p) => LosCalculations.GetWarhammerLos(caster, p).CoverType != LosCalculations.CoverType.LosBlocker);
		}
		if (NotCaster)
		{
			list.RemoveAll((BaseUnitEntity p) => p == base.Context.Caster);
		}
		if (list.Empty())
		{
			return;
		}
		foreach (BlueprintUnitFact fact in FilterNoFacts)
		{
			list.RemoveAll((BaseUnitEntity p) => p.Facts.Contains(fact));
		}
		if (FilterHaveAnyFact.Any())
		{
			list = list.Where((BaseUnitEntity unit) => unit.Facts.Contains((EntityFact fact) => FilterHaveAnyFact.Contains(fact.Blueprint))).ToList();
		}
		if (list.Count <= 0)
		{
			return;
		}
		if (ActionsOnRandomTarget)
		{
			BaseUnitEntity baseUnitEntity2 = list.Random(PFStatefulRandom.Mechanics);
			using (base.Context.PushTarget(baseUnitEntity2))
			{
				Actions.Run();
				return;
			}
		}
		foreach (BaseUnitEntity item in list)
		{
			using (base.Context.PushTarget(item))
			{
				Actions.Run();
			}
		}
	}
}
