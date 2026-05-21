using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Controllers.MapObjects;

public class PartyAwarenessController : IControllerTick, IController, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber
{
	private readonly HashSet<BaseUnitEntity> m_ForceUpdateCharacters = new HashSet<BaseUnitEntity>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (!partyAndPet.Movable.HasMotionThisSimulationTick && !m_ForceUpdateCharacters.Contains(partyAndPet))
			{
				continue;
			}
			m_ForceUpdateCharacters.Remove(partyAndPet);
			foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
			{
				PartAwarenessCheck awarenessCheck = mapObject.AwarenessCheck;
				if (awarenessCheck == null || !awarenessCheck.Settings.HiddenInDarkness || !Game.Instance.Player.Flashlight.FlashlightInUse)
				{
					HandleAwarenessCheck(mapObject, partyAndPet);
				}
			}
		}
	}

	public void ForceUpdateMapObject(MapObjectEntity mapObject)
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			HandleAwarenessCheck(mapObject, partyAndPet);
		}
	}

	private static void HandleAwarenessCheck(MapObjectEntity mapObject, BaseUnitEntity character)
	{
		if (mapObject.IsInFogOfWar)
		{
			return;
		}
		PartAwarenessCheck awarenessCheck = mapObject.AwarenessCheck;
		EntityPartsManager.PartsByTypeEnumerable<AbstractInteractionPart> interactions = mapObject.Interactions;
		bool flag = false;
		if (!awarenessCheck)
		{
			foreach (AbstractInteractionPart item in interactions)
			{
				if (IsInteractionForForceHighlight(item))
				{
					flag = true;
					break;
				}
			}
		}
		if ((awarenessCheck == null && !flag) || (awarenessCheck != null && awarenessCheck.Settings.HiddenInDarkness && Game.Instance.Player.Flashlight.FlashlightInUse && !awarenessCheck.IsRevealedByFlashlight))
		{
			return;
		}
		bool isAwarenessCheckPassed = mapObject.IsAwarenessCheckPassed;
		float num = character.DistanceTo(mapObject.ViewPosition);
		if (isAwarenessCheckPassed || flag)
		{
			if (!mapObject.WasHighlightedOnRevealAndNoticed && num < (float)ConfigRoot.Instance.SystemMechanics.StandartPerceptionRadius && !Game.Instance.Player.IsInCombat)
			{
				mapObject.OnEntityNoticed(character);
			}
		}
		else if (awarenessCheck.IsCheckAllowedFor(character) && num < awarenessCheck.Settings.Radius && character.Vision.HasLOS(mapObject))
		{
			RollAwareness(character, mapObject);
		}
		if (isAwarenessCheckPassed || !mapObject.IsAwarenessCheckPassed || !(mapObject is TrapObjectData trapObjectData))
		{
			return;
		}
		PartAwarenessCheck partAwarenessCheck = trapObjectData.LinkedTrap?.AwarenessCheck;
		if (partAwarenessCheck != null)
		{
			partAwarenessCheck.SetPassed(value: true);
			EventBus.RaiseEvent((IMapObjectEntity)trapObjectData.LinkedTrap, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(character);
			}, isCheckRuntime: true);
			trapObjectData.LinkedTrap.OnEntityNoticed(character);
		}
		PartAwarenessCheck partAwarenessCheck2 = trapObjectData.Device?.AwarenessCheck;
		if (partAwarenessCheck2 != null)
		{
			partAwarenessCheck2.SetPassed(value: true);
			EventBus.RaiseEvent((IMapObjectEntity)trapObjectData.Device, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(character);
			}, isCheckRuntime: true);
			trapObjectData.Device.OnEntityNoticed(character);
		}
	}

	private static bool IsInteractionForForceHighlight(AbstractInteractionPart i)
	{
		if (i is InteractionLootPart)
		{
			return i.Enabled;
		}
		return false;
	}

	private static void RollAwareness(BaseUnitEntity character, MapObjectEntity data)
	{
		PartAwarenessCheck awarenessCheck = data.AwarenessCheck;
		bool flag = awarenessCheck.Settings.Difficulty == SkillCheckDifficulty.AutoPass;
		if (!flag)
		{
			int difficulty = awarenessCheck.Settings.GetDifficulty();
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(character, StatType.SkillAwareness, difficulty)
			{
				Reason = data
			});
			Metrics.SkillCheck.Type(SkillCheckMetricsEvent.Types.Awareness).Initiator(character.Blueprint.AssetGuid).Target(data.Blueprint.AssetGuid)
				.Result(rulePerformSkillCheck.ResultIsSuccess)
				.Send();
			awarenessCheck.LastAwarenessValue[character.FromBaseUnitEntity()] = character.Actor.GetStat(StatType.SkillAwareness, null, default(StatContext), "RollAwareness");
			flag = rulePerformSkillCheck.ResultIsSuccess;
		}
		awarenessCheck.SetPassed(flag);
		if (flag)
		{
			EventBus.RaiseEvent((IMapObjectEntity)data, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(character);
			}, isCheckRuntime: true);
			if (data is TrapObjectData { TrappedObject: { } trappedObject })
			{
				trappedObject.OnEntityNoticed(character);
			}
		}
		else if (BuildModeUtility.IsDevelopment)
		{
			UtilityMessageBox.SendWarning($"Perception failed on {data}");
		}
	}

	public void HandleEntityPositionChanged()
	{
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn && EventInvokerExtensions.Entity is BaseUnitEntity baseUnitEntity && Game.Instance.Player.PartyAndPets.Contains(baseUnitEntity) && baseUnitEntity.IsInCombat)
		{
			m_ForceUpdateCharacters.Add(baseUnitEntity);
		}
	}
}
