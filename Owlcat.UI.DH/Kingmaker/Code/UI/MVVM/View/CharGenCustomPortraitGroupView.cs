using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitGroupView : View<CharGenPortraitGroupVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private CharGenCustomPortraitCreatorItemView m_CreatorItemPrefab;

	[SerializeField]
	private int m_ItemsInRow;

	private GridConsoleNavigationBehaviour m_Navigation;

	public IConsoleEntity ConsoleEntityProxy => m_Navigation;

	protected override void OnBind()
	{
		base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawMultiEntries(base.ViewModel.PortraitCollection, new List<MonoBehaviour> { m_Prefab, m_CreatorItemPrefab }).AddTo(this);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_Navigation == null)
		{
			m_Navigation = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		m_Navigation.Clear();
		m_Navigation.SetEntitiesGrid(m_WidgetList.Entries.Cast<IConsoleEntity>().ToList(), m_ItemsInRow);
	}

	public void FocusOnSelectedEntityOrFirst()
	{
		IConsoleNavigationEntity selectedEntity = GetSelectedEntity();
		if (selectedEntity == null)
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
		else
		{
			m_Navigation.FocusOnEntityManual(selectedEntity);
		}
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		CharGenPortraitSelectorItemView charGenPortraitSelectorItemView = m_WidgetList.Entries?.Where((IBindable e) => e is CharGenPortraitSelectorItemView).Cast<CharGenPortraitSelectorItemView>().FirstOrDefault((CharGenPortraitSelectorItemView i) => i.IsSelected);
		if (!(charGenPortraitSelectorItemView == null))
		{
			return charGenPortraitSelectorItemView;
		}
		return m_WidgetList.Entries?.FirstOrDefault() as IConsoleNavigationEntity;
	}
}
