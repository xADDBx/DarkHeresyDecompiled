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
	[SerializeField]
	private HintView m_SellHint;

	[SerializeField]
	private HintView m_ShowUnrelevantHint;

	[SerializeField]
	private HintView m_SelectMenuHint;

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

	public HintView GetSellHint()
	{
		return m_SellHint;
	}

	public HintView GetSelectContextMenuHint()
	{
		return m_SelectMenuHint;
	}

	public HintView GetUnrelevantHint()
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
