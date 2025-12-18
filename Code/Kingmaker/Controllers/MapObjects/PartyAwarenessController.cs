using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
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
		if ((awarenessCheck == null && !flag) || (awarenessCheck != null && awarenessCheck.Settings.HiddenInDarkness && !awarenessCheck.IsRevealedByFlashlight))
		{
			return;
		}
		bool isAwarenessCheckPassed = mapObject.IsAwarenessCheckPassed;
		float num = character.DistanceTo(mapObject.View.ViewTransform.position);
		if (isAwarenessCheckPassed || flag)
		{
			if (!mapObject.WasHighlightedOnRevealAndNoticed && num < (float)ConfigRoot.Instance.SystemMechanics.StandartPerceptionRadius && !Game.Instance.Player.IsInCombat)
			{
				mapObject.View.OnEntityNoticed(character);
			}
		}
		else if (awarenessCheck.IsCheckAllowedFor(character) && num < awarenessCheck.Settings.Radius && character.Vision.HasLOS(mapObject.View))
		{
			RollAwareness(character, mapObject);
		}
		if (!isAwarenessCheckPassed && mapObject.IsAwarenessCheckPassed && mapObject.View is TrapObjectView trapObjectView)
		{
			PartAwarenessCheck partAwarenessCheck = trapObjectView.LinkedTrap?.Data.AwarenessCheck;
			if (partAwarenessCheck != null)
			{
				partAwarenessCheck.IsPassed = true;
			}
			PartAwarenessCheck partAwarenessCheck2 = trapObjectView.Device?.Data.AwarenessCheck;
			if (partAwarenessCheck2 != null)
			{
				partAwarenessCheck2.IsPassed = true;
			}
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
			awarenessCheck.LastAwarenessValue[character.FromBaseUnitEntity()] = character.Skills.SkillAwareness;
			flag = rulePerformSkillCheck.ResultIsSuccess;
		}
		awarenessCheck.IsPassed = flag;
		if (flag)
		{
			EventBus.RaiseEvent((IMapObjectEntity)data, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(character);
			}, isCheckRuntime: true);
			if (data.View is TrapObjectView { TrappedObject: not null } trapObjectView)
			{
				trapObjectView.TrappedObject.View.OnEntityNoticed(character);
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
