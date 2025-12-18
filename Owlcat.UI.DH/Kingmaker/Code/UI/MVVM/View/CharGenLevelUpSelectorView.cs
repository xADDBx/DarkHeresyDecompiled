using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpSelectorView : View<SelectionGroupRadioVM<CharGenLevelUpSelectorBaseItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenLevelUpNestedListHeaderView m_HeaderViewPrefab;

	[SerializeField]
	private CharGenLevelUpSelectorCommonItemView m_ItemViewPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		RectTransform obj = (RectTransform)m_WidgetList.Container;
		obj.anchoredPosition = new Vector2(obj.anchoredPosition.x, 0f);
		m_WidgetList.DrawMultiEntries(base.ViewModel.EntitiesCollection, new List<MonoBehaviour> { m_HeaderViewPrefab, m_ItemViewPrefab });
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenLevelUpSelectorCommonItemView>().FirstOrDefault((CharGenLevelUpSelectorCommonItemView i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}
}
