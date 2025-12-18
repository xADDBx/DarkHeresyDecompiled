using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("01f32056e96f2784688b0ec0edcbdb51")]
public class RemoveDeathDoor : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Снимает DeathDoor с указанного юнита {Unit}";
	}

	public override string GetCaption()
	{
		return $"Remove DeathDoor condition from {Unit}";
	}

	protected override void RunAction()
	{
		Unit.GetValue().Remove<UnitPartDeathDoor>();
	}
}
