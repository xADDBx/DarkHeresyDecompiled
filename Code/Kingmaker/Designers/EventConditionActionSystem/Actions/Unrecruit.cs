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

[ComponentName("Actions/Unrecruit")]
[PlayerUpgraderAllowed(false)]
[AllowMultipleComponents]
[TypeId("7d6c4f7ff596e5e4086531c0f96ac650")]
public class Unrecruit : CompanionActionBase
{
	public ActionList? OnUnrecruit;

	protected virtual CompanionExState ExState => CompanionExState.InReserve;

	protected override void RunAction(BlueprintUnit unit)
	{
		UnrecruitUnit(unit, ExState, OnUnrecruit);
	}

	public static void UnrecruitUnit(BlueprintUnit unit, CompanionExState exState, ActionList? onUnrecruit)
	{
		BlueprintUnit unit = unit;
		BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity obj = Game.Instance.Player.AllCharacters.Where((BaseUnitEntity u) => u != mainCharacter).FirstOrDefault((BaseUnitEntity u) => unit == u.Blueprint) ?? throw new Exception($"No companion unit found when un-recruiting {unit}");
		DoUnrecruit(obj, exState, onUnrecruit);
		EventBus.RaiseEvent((IBaseUnitEntity)obj, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
		{
			h.HandleUnrecruit();
		}, isCheckRuntime: true);
	}

	private static void DoUnrecruit(BaseUnitEntity companion, CompanionExState exState, ActionList? onUnrecruit)
	{
		UnitPartCompanion obj = companion.GetOrCreate<UnitPartCompanion>() ?? throw new Exception("No companion part in " + companion.Name + ".");
		if (obj.State == CompanionState.ExCompanion)
		{
			throw new Exception("Companion " + companion.Name + " already lost, cannot unrecruit again.");
		}
		companion.StopMoving();
		Game.Instance.Player.RemoveCompanion(companion);
		obj.SetExState(exState);
		obj.SetState(CompanionState.ExCompanion);
		using (ContextData<RecruitedUnitData>.Request().Setup(companion))
		{
			onUnrecruit?.Run();
		}
	}

	public override string GetCaption()
	{
		return "Unrecruit InReserve(" + base.CaptionName + ")";
	}
}
