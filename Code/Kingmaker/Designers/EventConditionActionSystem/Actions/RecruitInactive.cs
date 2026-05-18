using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("857993ffeba11124699997a531336700")]
public class RecruitInactive : CompanionActionBase
{
	public ActionList? OnRecruit;

	protected override void RunAction(BlueprintUnit unit)
	{
		RecruitInactiveUnit(unit, OnRecruit);
	}

	public static void RecruitInactiveUnit(BlueprintUnit unit, ActionList? onRecruit)
	{
		BlueprintUnit unit = unit;
		BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.Where((BaseUnitEntity u) => u != mainCharacter).FirstOrDefault((BaseUnitEntity u) => unit == u.Blueprint);
		if (baseUnitEntity == null)
		{
			throw new Exception($"RecruitInactive failed. Companion is not in roster yet: {unit}");
		}
		UnitPartCompanion unitPartCompanion = baseUnitEntity.GetOptional<UnitPartCompanion>();
		if (unitPartCompanion == null)
		{
			Element.LogError("Actual companion with no UnitPartCompanion: " + baseUnitEntity.Name);
			unitPartCompanion = baseUnitEntity.GetOrCreate<UnitPartCompanion>();
		}
		else if (unitPartCompanion.State != CompanionState.ExCompanion)
		{
			throw new Exception("Attempted to double-recruit " + baseUnitEntity.Name);
		}
		if (unitPartCompanion.IsExForever)
		{
			throw new Exception($"Trying to recruit inactive companion {baseUnitEntity.Name} that is {unitPartCompanion.ExState}");
		}
		unitPartCompanion.SetState(CompanionState.Remote);
		baseUnitEntity.CombatGroup.Id = "<directly-controllable-unit>";
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleAddCompanion();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
		using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
		{
			onRecruit?.Run();
		}
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
		{
			h.HandleRecruit();
		}, isCheckRuntime: true);
	}

	public override string GetCaption()
	{
		return "Recruit to capital (" + base.CaptionName + ")";
	}
}
