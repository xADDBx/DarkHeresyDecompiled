using System.Linq;
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

	private bool m_IsInit;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetListMvvm.DrawEntries(base.ViewModel.EntitiesCollection, m_Prefab).AddTo(this);
	}

	public GridConsoleNavigationBehaviour GetNavigation(IConsoleNavigationOwner owner = null)
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		}
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetListMvvm.Entries.Cast<IConsoleEntity>().ToList(), 2);
		return m_NavigationBehaviour;
	}
}
