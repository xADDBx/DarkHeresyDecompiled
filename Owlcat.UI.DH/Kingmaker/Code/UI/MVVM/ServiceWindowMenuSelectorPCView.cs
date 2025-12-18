using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowMenuSelectorPCView : View<SelectionGroupRadioVM<ServiceWindowsMenuEntityVM>>
{
	[SerializeField]
	private List<ServiceWindowsMenuEntityPCView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<ServiceWindowsMenuEntityPCView>().ToList();
		}
	}

	protected override void OnBind()
	{
		for (int i = 0; i < m_MenuEntities.Count; i++)
		{
			if (i >= base.ViewModel.EntitiesCollection.Count)
			{
				m_MenuEntities[i].gameObject.SetActive(value: false);
			}
			else
			{
				m_MenuEntities[i].Bind(base.ViewModel.EntitiesCollection[i]);
			}
		}
	}
}
