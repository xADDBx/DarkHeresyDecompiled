using System;
using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public sealed class CombatTextCommonCreator : CombatTextCreator<CombatTextCommonView, CombatMessageBase>
{
	private Dictionary<CombatMessageBase, CombatTextCommonView> m_MessageToMessageView = new Dictionary<CombatMessageBase, CombatTextCommonView>();

	public override CombatTextCommonView Create(CombatMessageBase message)
	{
		if (m_MessageToMessageView.TryGetValue(message, out var value))
		{
			value.RenewTimer();
			return value;
		}
		value = base.Create(message);
		m_MessageToMessageView.Add(message, value);
		return value;
	}

	protected override void OnTextViewDisposed(CombatTextEntityBaseView<CombatMessageBase> combatText)
	{
		CombatMessageBase combatMessageBase = null;
		foreach (var (combatMessageBase3, combatTextCommonView2) in m_MessageToMessageView)
		{
			if (!(combatTextCommonView2 != combatText))
			{
				combatMessageBase = combatMessageBase3;
				break;
			}
		}
		if (combatMessageBase != null)
		{
			m_MessageToMessageView.Remove(combatMessageBase);
		}
	}
}
