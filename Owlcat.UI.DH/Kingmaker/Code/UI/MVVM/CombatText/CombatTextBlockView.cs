using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatText;

public class CombatTextBlockView : View<CombatTextBlockVM>
{
	[Header("Common Combat Texts")]
	[SerializeField]
	private FadeAnimator m_CombatTextContainerAnimator;

	[SerializeField]
	private CombatTextCommonCreator m_CombatTextCommonCreator;

	private List<CombatMessageBase> m_MessagesList = new List<CombatMessageBase>();

	private IDisposable m_MessageDelay;

	protected override void OnBind()
	{
		base.ViewModel.CombatMessage.Subscribe(OnCombatMessage).AddTo(this);
		m_CombatTextCommonCreator.Clear();
	}

	protected override void OnUnbind()
	{
		m_CombatTextCommonCreator.Clear();
	}

	private void OnCombatMessage(CombatMessageBase message)
	{
		m_MessageDelay?.Dispose();
		if (!(message?.GetSprite() == null) || !string.IsNullOrEmpty(message?.GetText()))
		{
			m_MessagesList.Add(message);
			m_MessageDelay = DelayedInvoker.InvokeInTime(AddCombatMessage, 0.1f);
		}
	}

	private void AddCombatMessage()
	{
		if (m_MessagesList.Count == 1)
		{
			AddCombatText(m_MessagesList.First());
		}
		else
		{
			for (int i = 0; i < m_MessagesList.Count; i++)
			{
				AddCombatText(m_MessagesList[i]);
			}
		}
		m_MessagesList.Clear();
	}

	private void AddCombatText(CombatMessageBase message)
	{
		m_CombatTextCommonCreator.Create(message);
	}
}
