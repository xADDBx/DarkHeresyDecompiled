using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter.Actions;

[Serializable]
[TypeId("232d2d74e0eb482394de8f3bcca0332e")]
public sealed class JoinEncounter : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintEncounter> Encounter = new BpRef<BlueprintEncounter>();

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool SetCombatGroup;

	[ShowIf("SetCombatGroup")]
	public int CombatGroup;

	public override string GetCaption()
	{
		return $"Join {Unit} to encounter {Encounter}";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = (Unit.GetValue() as BaseUnitEntity) ?? throw new InvalidOperationException();
		baseUnitEntity.GetOrCreate<PartEncounter>().SetupOnJoin(Encounter, SetCombatGroup ? new int?(CombatGroup) : null);
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null && Encounter == current.Blueprint)
		{
			current.AddParticipant(baseUnitEntity);
		}
	}
}
