using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerSelectorView : View<SelectionGroupRadioVM<CharGenCareerSelectionItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenCareerItemView m_ItemViewPrefab;

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
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemViewPrefab);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenCareerItemView>().FirstOrDefault((CharGenCareerItemView i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}
}
