using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ComponentName("Dialogue/DialogueNotAnsweringTrigger")]
[TypeId("71259eb61496413bae87af31c051ef6a")]
public class DialogueNotAnsweringTrigger : EntityFactComponentDelegate, IDialogStartHandler, ISubscriber, ISelectAnswerHandler, IDialogAnswersShownHandler, IGameTimeChangedHandler
{
	[SerializeField]
	private int m_TimerValue;

	[SerializeField]
	private ActionList m_Actions;

	private BlueprintDialog m_Dialog;

	private float m_TimeSinceNotAnswering;

	private bool m_CanStart;

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		m_Dialog = dialog;
	}

	public void HandleSelectAnswer(BlueprintAnswer answer)
	{
		m_Dialog = null;
	}

	public void HandleAnswersShown()
	{
		m_CanStart = true;
	}

	public void HandleNonGameTimeChanged()
	{
		if (Game.Instance.CurrentModeType == GameModeType.Dialog && m_CanStart)
		{
			BlueprintDialog dialog = m_Dialog;
			if (dialog != null && dialog.TurnPlayer)
			{
				m_TimeSinceNotAnswering += Time.deltaTime;
			}
		}
		if (m_TimeSinceNotAnswering >= (float)m_TimerValue)
		{
			m_Actions.Run();
			m_Dialog = null;
		}
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
	}
}
