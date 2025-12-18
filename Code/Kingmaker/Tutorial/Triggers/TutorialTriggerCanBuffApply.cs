using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Tutorial.Solvers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("Triggers on enter one of the specified areas, if buff could be applied\n`t|SolutionAbility` - ability with buff\n`t|SolutionUnit` - unit who can cast ability\n`t|TargetUnit` - tank unit (if `Check Tank Stat` is on)")]
[TypeId("89de4bacd6af9bf478054dc6aeddb93c")]
public class TutorialTriggerCanBuffApply : TutorialTrigger, IAreaHandler, ISubscriber
{
	[SerializeField]
	private BlueprintAreaReference[] m_TriggerAreas;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[InfoBox("Check if stat modificator from abilityBuff is greater than current modificator on top-AC unit (Tank)\nDesigned for Barkskin buff. Could work badly with some specific stat modifiers. Contact Dev, if not sure")]
	private bool m_CheckTankStat;

	[SerializeField]
	private bool m_AllowItemsWithSpell;

	public ReferenceArrayProxy<BlueprintArea> TriggerAreas
	{
		get
		{
			BlueprintReference<BlueprintArea>[] triggerAreas = m_TriggerAreas;
			return triggerAreas;
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		using (ProfileScope.New("Tutor trigger. Pre-Buff"))
		{
			if (!TriggerAreas.HasReference(Game.Instance.CurrentlyLoadedArea))
			{
				return;
			}
			BaseUnitEntity tank = GetTank();
			BlueprintAbility blueprint = m_Ability.Get();
			AddStatModifier addStatModifier = null;
			BlueprintBuff blueprintBuff = null;
			if (m_CheckTankStat)
			{
				blueprintBuff = (blueprint.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.FirstOrDefault((GameAction x) => x is ContextActionApplyBuff) as ContextActionApplyBuff)?.Buff;
				if (blueprintBuff != null)
				{
					addStatModifier = blueprintBuff.GetComponent<AddStatModifier>();
				}
			}
			IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
			while (enumerator.MoveNext())
			{
				AbilityData current = enumerator.Current;
				if (current == null || !current.Blueprint.SameAbility(blueprint) || (!m_AllowItemsWithSpell && current.SourceItem != null))
				{
					continue;
				}
				if (m_CheckTankStat && addStatModifier != null && !addStatModifier.Descriptor.IsStackable())
				{
					AbilityExecutionContext parent = current.ClaimExecutionContext(tank);
					using (MechanicsContext mechanicsContext = MechanicsContext.Claim(blueprintBuff, tank, null, parent))
					{
						mechanicsContext.Recalculate();
						ModifiableValue stat = tank.Stats.GetStat(addStatModifier.Stat);
						int num = addStatModifier.Value.Calculate(mechanicsContext);
						int num2 = 0;
						foreach (Modifier modifier in stat.GetModifiers(addStatModifier.Descriptor))
						{
							if (!modifier.Stackable && num2 < modifier.Value)
							{
								num2 = modifier.Value;
							}
						}
						if (num2 < num)
						{
							Trigger(current, tank);
							break;
						}
					}
					continue;
				}
				Trigger(current);
				break;
			}
		}
	}

	private void Trigger(AbilityData ability, BaseUnitEntity tankUnit = null)
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			if (tankUnit != null)
			{
				context.TargetUnit = tankUnit;
			}
			context.SolutionAbility = ability;
			context.SolutionUnit = ability.Caster as BaseUnitEntity;
		});
	}

	private BaseUnitEntity GetTank()
	{
		if (m_CheckTankStat)
		{
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.AddRange(Game.Instance.Player.PartyAndPets);
			list.Sort((BaseUnitEntity unit1, BaseUnitEntity unit2) => 0);
			return list[0];
		}
		return null;
	}
}
