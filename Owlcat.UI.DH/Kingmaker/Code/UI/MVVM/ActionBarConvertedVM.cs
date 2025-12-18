using System;
using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarConvertedVM : ViewModel
{
	public readonly List<ActionBarSlotVM> Slots = new List<ActionBarSlotVM>();

	private readonly Action m_OnClose;

	public ActionBarConvertedVM(List<MechanicActionBarSlotSpontaneusConvertedSpell> list, Action onClose)
	{
		m_OnClose = onClose;
		for (int i = 0; i < list.Count; i++)
		{
			Slots.Add(new ActionBarSlotVM(list[i], i));
		}
	}

	protected override void OnDispose()
	{
		Slots.ForEach(delegate(ActionBarSlotVM s)
		{
			s.Dispose();
		});
		Slots.Clear();
	}

	public void Close()
	{
		m_OnClose?.Invoke();
	}
}
