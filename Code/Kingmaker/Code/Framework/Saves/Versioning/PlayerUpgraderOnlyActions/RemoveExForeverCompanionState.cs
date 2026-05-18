using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Framework.Saves.Versioning.PlayerUpgraderOnlyActions;

[TypeId("1c40356b09df4be59a377d21ba80cd8d")]
public class RemoveExForeverCompanionState : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitReference? m_CompanionBlueprint;

	protected override void RunActionOverride()
	{
		BlueprintUnit bpCompanion = m_CompanionBlueprint?.Get();
		if (bpCompanion != null)
		{
			BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
			UnitPartCompanion unitPartCompanion = Game.Instance.Player.AllCharacters.Where((BaseUnitEntity u) => u != mainCharacter).FirstOrDefault((BaseUnitEntity u) => u.Blueprint == bpCompanion)?.GetOptional<UnitPartCompanion>();
			if (unitPartCompanion != null && unitPartCompanion.IsExForever)
			{
				unitPartCompanion.SetExState(CompanionExState.InReserve);
			}
		}
	}

	public override string GetCaption()
	{
		return "Make ex-state InReserve (" + m_CompanionBlueprint?.Get().NameSafe() + ")";
	}
}
