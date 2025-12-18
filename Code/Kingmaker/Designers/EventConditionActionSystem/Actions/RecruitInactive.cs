using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("857993ffeba11124699997a531336700")]
public class RecruitInactive : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public ActionList OnRecruit;

	public BlueprintUnit CompanionBlueprint
	{
		get
		{
			return m_CompanionBlueprint?.Get();
		}
		set
		{
			m_CompanionBlueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	protected override void RunAction()
	{
		RecruitInactiveUnit(CompanionBlueprint, OnRecruit);
	}

	public static void RecruitInactiveUnit(BlueprintUnit unit, ActionList onRecruit)
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.FirstOrDefault((BaseUnitEntity c) => c.Blueprint == unit);
		if (baseUnitEntity != null && baseUnitEntity.GetOptional<UnitPartCompanion>() != null && baseUnitEntity.GetOptional<UnitPartCompanion>().State != CompanionState.ExCompanion)
		{
			Element.LogError($"Attempted to double-recruit {unit}");
			return;
		}
		BaseUnitEntity baseUnitEntity2 = ((baseUnitEntity != null && baseUnitEntity.GetOptional<UnitPartCompanion>()?.State == CompanionState.ExCompanion) ? baseUnitEntity : null);
		if (baseUnitEntity2 == null)
		{
			baseUnitEntity2 = Game.Instance.Controllers.EntitySpawner.SpawnUnit(unit, Vector3.zero, Quaternion.identity, Game.Instance.Player.CrossSceneState);
			baseUnitEntity2.IsInGame = false;
			baseUnitEntity2.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.Remote);
			int experience = Game.Instance.Player.MainCharacterEntity.ToBaseUnitEntity().Progression.Experience;
			baseUnitEntity2.Progression.AdvanceExperienceTo(experience, log: false);
		}
		else
		{
			baseUnitEntity2.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.Remote);
		}
		baseUnitEntity2.CombatGroup.Id = "<directly-controllable-unit>";
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity2, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleAddCompanion();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity2, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
		using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity2))
		{
			onRecruit.Run();
		}
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity2, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
		{
			h.HandleRecruit();
		}, isCheckRuntime: true);
	}

	public override string GetCaption()
	{
		return $"Recruit ({CompanionBlueprint}) to capital";
	}
}
