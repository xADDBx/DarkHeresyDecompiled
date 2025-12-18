using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationPartConsoleView : VendorReputationPartView<VendorReputationForItemWindowConsoleView>
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private ConsoleHint m_SellHint;

	[SerializeField]
	private ConsoleHint m_ShowUnrelevantHint;

	[SerializeField]
	private ConsoleHint m_SelectMenuHint;

	[SerializeField]
	private TextMeshProUGUI m_SelectMenuText;

	[SerializeField]
	private Image m_ContextMenuPlace;

	[SerializeField]
	protected GameObject m_ReputationPartTabsBlock;

	private readonly ReactiveProperty<bool> m_CanSell = new ReactiveProperty<bool>();

	public IConsoleEntity m_CurrentFocus;

	protected ContextMenuCollectionEntity m_SelectAllEntity;

	protected ContextMenuCollectionEntity m_UnselectAllEntity;

	private readonly ReactiveCommand<Unit> m_OnNeedRefocus = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<bool> CanSell => m_CanSell;

	public Observable<Unit> OnNeedRefocus => m_OnNeedRefocus;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		SetupContextMenu();
		base.ViewModel.HasItemsToSell.Subscribe(delegate
		{
			UpdateContextMenu();
		}).AddTo(this);
		base.ViewModel.CanSellCargo.Subscribe(delegate
		{
			UpdateContextMenu();
		}).AddTo(this);
	}

	private void ChangeView()
	{
		m_SelectorView.SetNextTab();
		m_ReputationForItemWindowPCView.ForceScrollToTop();
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		m_NavigationBehaviour.AddEntityHorizontal(m_ReputationForItemWindowPCView.GetNavigation());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		m_CurrentFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_ReputationForItemWindowPCView.ForceScrollToTop();
		return m_NavigationBehaviour;
	}

	public void SetupContextMenu()
	{
		UIVendor vendor = UIStrings.Instance.Vendor;
		m_SelectAllEntity = new ContextMenuCollectionEntity(vendor.SelectAllRelevant, base.ViewModel.SelectAll, condition: true, base.ViewModel.HasItemsToSell.CurrentValue);
		m_UnselectAllEntity = new ContextMenuCollectionEntity(vendor.UnselectAllRelevant, base.ViewModel.UnselectAll, condition: true, base.ViewModel.CanSellCargo.CurrentValue);
		base.ViewModel.SetContextMenu(new List<ContextMenuCollectionEntity> { m_SelectAllEntity, m_UnselectAllEntity });
	}

	public void HandleContextMenu()
	{
		m_ContextMenuPlace.ShowContextMenu(base.ViewModel.ContextMenu?.CurrentValue);
	}

	public void SellCargo()
	{
		base.ViewModel.SellCargo();
	}

	public ConsoleHint GetSellHint()
	{
		return m_SellHint;
	}

	public ConsoleHint GetSelectContextMenuHint()
	{
		return m_SelectMenuHint;
	}

	public ConsoleHint GetUnrelevantHint()
	{
		return m_ShowUnrelevantHint;
	}

	public void SetUnrelevantToggle()
	{
		m_ShowUnrelevantToggle.Set(!m_ShowUnrelevantToggle.IsOn.CurrentValue);
	}

	private void UpdateContextMenu()
	{
		m_SelectAllEntity.ForceUpdateInteractive(base.ViewModel.HasItemsToSell.CurrentValue);
		m_UnselectAllEntity.ForceUpdateInteractive(base.ViewModel.CanSellCargo.CurrentValue);
	}
}
