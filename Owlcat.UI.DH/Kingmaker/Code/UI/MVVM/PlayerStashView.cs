using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PlayerStashView : View<PlayerStashVM>
{
	[SerializeField]
	public ChestStashView m_ChestStashView;

	[SerializeField]
	public ItemsFilterBaseView m_ItemsFilter;

	[SerializeField]
	private TextMeshProUGUI m_HeaderTitle;

	[SerializeField]
	protected OwlcatMultiButton m_CloseButton;

	[Header("Player Inventory")]
	[SerializeField]
	protected InventoryStashView m_Inventory;

	[Header("Stashes Title")]
	[SerializeField]
	private TextMeshProUGUI m_ChestStashTitle;

	[SerializeField]
	private TextMeshProUGUI m_PlayerStashTitle;

	public void Initialize()
	{
		m_ChestStashView.Initialize();
		m_Inventory.Initialize();
		m_ItemsFilter.Initialize();
	}

	protected override void OnBind()
	{
		Show();
		m_ChestStashView.Bind(base.ViewModel.ContextLoot[0]);
		m_Inventory.Bind(base.ViewModel.StashVM);
		m_ItemsFilter.Bind(base.ViewModel.ItemsFilter);
		m_HeaderTitle.text = base.ViewModel.LootDisplayName;
		m_ChestStashTitle.text = UIStrings.Instance.LootWindow.LootPlayerChest;
		m_PlayerStashTitle.text = UIStrings.Instance.LootWindow.LootSharedStash;
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.Or(null)?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
