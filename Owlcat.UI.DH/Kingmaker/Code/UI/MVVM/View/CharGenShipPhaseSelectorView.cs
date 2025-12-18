using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenShipPhaseSelectorView : View<SelectionGroupRadioVM<CharGenShipItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenShipPhaseSelectorItemView m_ItemViewPrefab;

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemViewPrefab, unused: true);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenShipPhaseSelectorItemView>().FirstOrDefault((CharGenShipPhaseSelectorItemView i) => i.GetViewModel() == base.ViewModel.SelectedEntity.Value);
	}
}
