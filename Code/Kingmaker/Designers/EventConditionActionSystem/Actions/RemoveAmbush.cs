using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("a06a9d1874eec634e948de11eb01a3da")]
public class RemoveAmbush : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private bool m_ExitStealth;

	public override string GetCaption()
	{
		return "Remove ambush from " + m_Unit?.ToString() + (m_ExitStealth ? " and exit stealth" : "");
	}

	protected override void RunAction()
	{
		if (!(m_Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {m_Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		baseUnitEntity.Stealth.InAmbush = false;
		if (m_ExitStealth)
		{
			baseUnitEntity.Stealth.ShouldExitStealth = true;
			baseUnitEntity.Stealth.WantActivate = false;
			if (Game.Instance.CurrentModeType != GameModeType.Default)
			{
				baseUnitEntity.Stealth.Active = false;
				baseUnitEntity.Stealth.Clear();
			}
		}
	}
}
