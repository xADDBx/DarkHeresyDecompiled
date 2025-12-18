using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Unrecruit")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("7d6c4f7ff596e5e4086531c0f96ac650")]
public class Unrecruit : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public ActionList OnUnrecruit;

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
		UnrecruitUnit(CompanionBlueprint, OnUnrecruit);
	}

	public static void UnrecruitUnit(BlueprintUnit unit, ActionList onUnrecruit)
	{
		BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.Where((BaseUnitEntity u) => u != mainCharacter).FirstOrDefault((BaseUnitEntity u) => unit == u.Blueprint);
		if (baseUnitEntity == null)
		{
			Element.LogError("No companion unit found when unrecruiting {0}", unit);
			return;
		}
		DoUnrecruit(baseUnitEntity, onUnrecruit);
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
		{
			h.HandleUnrecruit();
		}, isCheckRuntime: true);
	}

	private static void DoUnrecruit(BaseUnitEntity companion, ActionList onUnrecruit)
	{
		UnitPartCompanion optional = companion.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.ExCompanion)
		{
			Element.LogError("Companion {0} already lost, cannot unrecruit again.", companion);
		}
		if ((bool)companion.View)
		{
			companion.View.StopMoving();
		}
		Game.Instance.Player.RemoveCompanion(companion);
		companion.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
		using (ContextData<RecruitedUnitData>.Request().Setup(companion))
		{
			onUnrecruit?.Run();
		}
	}

	private bool IsCompanion(BlueprintUnit unit)
	{
		return unit == CompanionBlueprint;
	}

	public override string GetCaption()
	{
		return $"Unrecruit ({CompanionBlueprint})";
	}
}
