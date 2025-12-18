using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsMenuSelectorBaseView : View<SelectionGroupRadioVM<SettingsMenuEntityVM>>, IInitializable
{
	[SerializeField]
	private List<SettingsMenuEntityBaseView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<SettingsMenuEntityBaseView>().ToList();
		}
	}

	protected override void OnBind()
	{
		for (int i = 0; i < m_MenuEntities.Count; i++)
		{
			if (i >= base.ViewModel.EntitiesCollection.Count)
			{
				m_MenuEntities[i].gameObject.SetActive(value: false);
				continue;
			}
			m_MenuEntities[i].gameObject.SetActive(value: true);
			m_MenuEntities[i].Bind(base.ViewModel.EntitiesCollection[i]);
		}
	}

	public void OnNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void OnPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}
}
