using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Actions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.AreaEffects;

public sealed class AreaEffectDescription
{
	private readonly Dictionary<AreaEffectEventType, AreaEffectDescriptionEntry[]> _eventEntries = new Dictionary<AreaEffectEventType, AreaEffectDescriptionEntry[]>();

	public readonly BlueprintAreaEffect Blueprint;

	public AreaEffectDescriptionEntry[] GetEffect(AreaEffectEventType @event)
	{
		return _eventEntries.GetValueOrDefault(@event) ?? Array.Empty<AreaEffectDescriptionEntry>();
	}

	public AreaEffectDescription(BlueprintAreaEffect blueprint)
	{
		Blueprint = blueprint;
		List<AreaEffectLogic> value;
		using (CollectionPool<List<AreaEffectLogic>, AreaEffectLogic>.Get(out value))
		{
			CollectAreaEffectLogic(blueprint.ComponentsArray, value);
			List<AreaEffectDescriptionEntry> value2;
			using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value2))
			{
				List<AreaEffectDescriptionEntry> value3;
				using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value3))
				{
					List<AreaEffectDescriptionEntry> value4;
					using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value4))
					{
						List<AreaEffectDescriptionEntry> value5;
						using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value5))
						{
							List<AreaEffectDescriptionEntry> value6;
							using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value6))
							{
								List<AreaEffectDescriptionEntry> value7;
								using (CollectionPool<List<AreaEffectDescriptionEntry>, AreaEffectDescriptionEntry>.Get(out value7))
								{
									foreach (AreaEffectLogic item in value)
									{
										CollectEffects(item, value2, value3, value4, value5, value6, value7);
									}
									if (value2.Count > 0)
									{
										_eventEntries[AreaEffectEventType.Enter] = value2.ToArray();
									}
									if (value3.Count > 0)
									{
										_eventEntries[AreaEffectEventType.Exit] = value3.ToArray();
									}
									if (value4.Count > 0)
									{
										_eventEntries[AreaEffectEventType.Move] = value4.ToArray();
									}
									if (value5.Count > 0)
									{
										_eventEntries[AreaEffectEventType.EndRound] = value5.ToArray();
									}
									if (value6.Count > 0)
									{
										_eventEntries[AreaEffectEventType.StartTurn] = value6.ToArray();
									}
									if (value7.Count > 0)
									{
										_eventEntries[AreaEffectEventType.EndTurn] = value7.ToArray();
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public bool IsHarmful()
	{
		foreach (AreaEffectDescriptionEntry[] value in _eventEntries.Values)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].IsHarmful)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void CollectAreaEffectLogic(BlueprintComponent[] components, List<AreaEffectLogic> list)
	{
		foreach (BlueprintComponent blueprintComponent in components)
		{
			if (blueprintComponent is AreaEffectLogic item)
			{
				list.Add(item);
			}
			else if (blueprintComponent is AreaEffectClusterComponent areaEffectClusterComponent)
			{
				CollectAreaEffectLogic(areaEffectClusterComponent.ClusterLogicBlueprint.ComponentsArray, list);
			}
		}
	}

	private static void CollectEffects(BlueprintComponent component, List<AreaEffectDescriptionEntry> enter, List<AreaEffectDescriptionEntry> exit, List<AreaEffectDescriptionEntry> move, List<AreaEffectDescriptionEntry> endRound, List<AreaEffectDescriptionEntry> startTurn, List<AreaEffectDescriptionEntry> endTurn)
	{
		if (component is AreaEffectBuff areaEffectBuff)
		{
			enter.Add(new AreaEffectDescriptionEntry(areaEffectBuff.Buff, areaEffectBuff.Condition.HasConditions));
		}
		else if (component is AreaEffectRunAction areaEffectRunAction)
		{
			CollectEntriesFromActions(areaEffectRunAction.UnitEnter, enter);
			CollectEntriesFromActions(areaEffectRunAction.UnitExit, exit);
			CollectEntriesFromActions(areaEffectRunAction.UnitMove, move);
			CollectEntriesFromActions(areaEffectRunAction.Round, endRound);
			CollectEntriesFromActions(areaEffectRunAction.OnUnitTurnStart, startTurn);
			CollectEntriesFromActions(areaEffectRunAction.OnUnitTurnEnd, endTurn);
		}
	}

	private static void CollectEntriesFromActions(ActionList actions, List<AreaEffectDescriptionEntry> list, bool conditional = false)
	{
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			if (gameAction is ContextActionDealDamage damage)
			{
				list.Add(new AreaEffectDescriptionEntry(damage, conditional));
			}
			else if (gameAction is ContextActionApplyBuff contextActionApplyBuff)
			{
				list.Add(new AreaEffectDescriptionEntry(contextActionApplyBuff.Buff, conditional));
			}
			else if (gameAction is ContextActionApplyDOT contextActionApplyDOT)
			{
				list.Add(new AreaEffectDescriptionEntry(contextActionApplyDOT.Type, conditional));
			}
			else if (gameAction is Conditional conditional2)
			{
				CollectEntriesFromActions(conditional2.IfTrue, list, conditional: true);
				CollectEntriesFromActions(conditional2.IfFalse, list, conditional: true);
			}
		}
	}
}
