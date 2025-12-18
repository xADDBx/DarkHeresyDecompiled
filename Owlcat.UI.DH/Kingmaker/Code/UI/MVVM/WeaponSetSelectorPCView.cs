using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponSetSelectorPCView : View<SelectionGroupRadioVM<WeaponSetVM>>
{
	[SerializeField]
	protected List<WeaponSetBaseView> m_WeaponSetViews;

	protected override void OnBind()
	{
		int i;
		for (i = 0; i < m_WeaponSetViews.Count; i++)
		{
			m_WeaponSetViews[i].Bind(base.ViewModel.EntitiesCollection.FirstOrDefault((WeaponSetVM e) => e.Index == i));
		}
	}

	public void RefreshItems()
	{
		m_WeaponSetViews.ForEach(delegate(WeaponSetBaseView ws)
		{
			ws.RefreshItems();
		});
	}
}
