using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatEndWindowVM : ViewModel
{
	private readonly ReactiveProperty<CombatEndReason> m_CombatEndReason = new ReactiveProperty<CombatEndReason>();

	private readonly ReactiveProperty<int> m_GainedXp = new ReactiveProperty<int>();

	private Action<bool> m_CloseCallback;

	public ReadOnlyReactiveProperty<CombatEndReason> CombatEndReason => m_CombatEndReason;

	public ReadOnlyReactiveProperty<int> GainedXp => m_GainedXp;

	public CombatEndWindowVM(Action<bool> closeCallback, CombatEndReason reason)
	{
		m_CloseCallback = closeCallback;
		m_CombatEndReason.Value = reason;
		CalculateGainedXp();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Close(bool endCombat)
	{
		m_CloseCallback?.Invoke(endCombat);
		m_CloseCallback = null;
	}

	private void CalculateGainedXp()
	{
		int value = 0;
		m_GainedXp.Value = value;
	}
}
