using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("32f7877eca56e284cbb0de719359b21b")]
public class EtudeBracketPreventDirectControl : EtudeBracketTrigger
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		if (!Unit || !Unit.CanEvaluate())
		{
			return;
		}
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Etude {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.PreventDirectControl.Retain();
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	protected override void OnExit()
	{
		if (!Unit || !Unit.CanEvaluate())
		{
			return;
		}
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Etude {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.PreventDirectControl.Release();
		}
	}
}
