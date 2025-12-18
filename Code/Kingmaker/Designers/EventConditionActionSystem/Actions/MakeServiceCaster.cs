using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("670301b4c7fedcb418f0d7eeb303ebb0")]
public class MakeServiceCaster : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Делает юнита {Unit} кастером для чтения свитков в хабе";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity unit))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			Game.Instance.State.LoadedAreaState.ServiceCaster = unit.FromBaseUnitEntity();
		}
	}

	public override string GetCaption()
	{
		return "Make " + Unit?.ToString() + " service caster";
	}
}
