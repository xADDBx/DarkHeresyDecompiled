using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BuffListVM<TBuffVM> : ViewModel where TBuffVM : IDisposable
{
	private readonly List<TBuffVM> m_BuffsList = new List<TBuffVM>();

	private readonly ReactiveProperty<IReadOnlyList<TBuffVM>> m_Buffs;

	private readonly Func<Buff, TBuffVM> m_GetBuffVM;

	public ReadOnlyReactiveProperty<IReadOnlyList<TBuffVM>> Buffs => m_Buffs;

	public int Count => m_BuffsList.Count;

	public BuffListVM(ReadOnlyReactiveProperty<IReadOnlyList<Buff>> buffs, Func<Buff, TBuffVM> getBuffVM)
	{
		m_Buffs = new ReactiveProperty<IReadOnlyList<TBuffVM>>(m_BuffsList);
		m_GetBuffVM = getBuffVM;
		buffs.Subscribe(HandleBuffsChanged).AddTo(this);
	}

	protected override void OnDispose()
	{
		ClearBuffs();
	}

	private void ClearBuffs()
	{
		foreach (TBuffVM buffs in m_BuffsList)
		{
			buffs.Dispose();
		}
		m_BuffsList.Clear();
	}

	private void HandleBuffsChanged(IReadOnlyList<Buff> buffs)
	{
		ClearBuffs();
		foreach (Buff buff in buffs)
		{
			m_BuffsList.Add(m_GetBuffVM(buff));
		}
		m_Buffs.ForceNotify();
	}
}
