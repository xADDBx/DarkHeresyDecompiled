using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Common;

public class MenuBaseView : View<MenuVM>
{
	[SerializeField]
	private List<MenuEntityView> m_MenuEntities;

	[SerializeField]
	private FlexibleLensSelectorView m_LensSelectorView;

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
		if (!(m_LensSelectorView != null))
		{
			return;
		}
		m_LensSelectorView.Bind(base.ViewModel.Selector);
		base.ViewModel.SelectedEntity.Subscribe(delegate
		{
			int num = m_MenuEntities.FindIndex((MenuEntityView e) => e.ViewModel.IsSelected.Value);
			if (num >= 0 && m_LensSelectorView.CurrentTabIndex != num)
			{
				m_LensSelectorView.ForceFocus(m_MenuEntities[num].RectTransform);
			}
		}).AddTo(this);
	}
}
