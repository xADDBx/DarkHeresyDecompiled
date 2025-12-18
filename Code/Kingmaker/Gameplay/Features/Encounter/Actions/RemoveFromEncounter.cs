using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter.Actions;

[TypeId("d335b9a789db4b418c94540a1db89ee8")]
public class RemoveFromEncounter : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetCaption()
	{
		return $"Remove {Unit} from active encounter";
	}

	protected override void RunAction()
	{
		((Unit.GetValue() as BaseUnitEntity) ?? throw new InvalidOperationException()).Remove<PartEncounter>();
	}
}
