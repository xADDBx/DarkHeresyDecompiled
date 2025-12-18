using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenDefaultPortraitGroupView : View<CharGenPortraitGroupVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private ExpandableElement m_ExpandableElement;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private int m_PortraitsInRow = 7;

	private GridConsoleNavigationBehaviour m_Navigation;

	public bool IsExpanded
	{
		get
		{
			if (base.ViewModel != null)
			{
				return base.ViewModel.Expanded.CurrentValue;
			}
			return false;
		}
	}

	public PortraitCategory PortraitCategory => base.ViewModel?.PortraitCategory ?? PortraitCategory.None;

	public IConsoleEntity ConsoleEntityProxy => m_Navigation;

	protected override void OnBind()
	{
		m_ExpandableElement.Or(null)?.Initialize(delegate
		{
			base.ViewModel.SetExpanded(isExpanded: true);
		}, delegate
		{
			base.ViewModel.SetExpanded(isExpanded: false);
		});
		m_ExpandableElement.AddTo(this);
		if (m_Label != null)
		{
			m_Label.text = UtilityChargen.GetCharGenPortraitCategoryLabel(base.ViewModel.PortraitCategory);
		}
		base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		base.ViewModel.Expanded.Subscribe(delegate(bool expanded)
		{
			if (expanded)
			{
				m_ExpandableElement.Or(null)?.Expand();
			}
			else
			{
				m_ExpandableElement.Or(null)?.Collapse();
			}
		}).AddTo(this);
		DrawEntities();
	}

	private void UpdateNavigation()
	{
		if (m_Navigation == null)
		{
			m_Navigation = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		m_Navigation.Clear();
		m_Navigation.SetEntitiesGrid(m_WidgetList.Entries.Cast<IConsoleEntity>().ToList(), m_PortraitsInRow);
		m_Navigation.InsertRow<ExpandableElement>(0, m_ExpandableElement);
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

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.PortraitCollection, m_Prefab).AddTo(this);
		UpdateNavigation();
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries?.Cast<CharGenPortraitSelectorItemView>().FirstOrDefault((CharGenPortraitSelectorItemView i) => i.IsSelected);
	}
}
