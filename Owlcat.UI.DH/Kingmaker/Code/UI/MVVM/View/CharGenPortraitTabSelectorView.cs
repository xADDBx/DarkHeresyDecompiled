using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitTabSelectorView : View<SelectionGroupRadioVM<CharGenPortraitTabVM>>
{
	[SerializeField]
	private WidgetList m_WidgetListMvvm;

	[SerializeField]
	private CharGenPortraitTabView m_Prefab;

	protected override void OnBind()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetListMvvm.DrawEntries(base.ViewModel.EntitiesCollection, m_Prefab).AddTo(this);
	}
}
