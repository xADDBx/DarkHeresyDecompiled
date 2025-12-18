using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenBackgroundBasePhaseSelectorView : View<SelectionGroupRadioVM<CharGenBackgroundBaseItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM> m_ItemViewPrefab;

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemViewPrefab, unused: true).AddTo(this);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM>>().FirstOrDefault((CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM> i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}
}
