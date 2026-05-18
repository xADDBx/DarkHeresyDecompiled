using System;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.Vendor;

public class VendorTradeView : View<VendorTradeViewVM>
{
	[Header("Vendor description")]
	[SerializeField]
	protected TextMeshProUGUI m_VendorName;

	[SerializeField]
	protected Image m_VendorPortrait;

	[SerializeField]
	protected GameObject m_VendorNoPortraitEffect;

	[SerializeField]
	protected TextMeshProUGUI m_VendorNoPortraitNoDataText;

	[SerializeField]
	protected TextMeshProUGUI m_FactionName;

	[SerializeField]
	protected Image m_FactionIcon;

	[SerializeField]
	protected Image m_FactionIconBottom;

	[Header("Vendor Block")]
	[SerializeField]
	private ItemSlotsGroupView m_VendorGroup;

	[SerializeField]
	private ItemsFilterBaseView m_VendorItemsFilter;

	[SerializeField]
	private VendorSlotPCView m_VendorSlotPrefab;

	[SerializeField]
	private OwlcatToggle m_ShowTrashToggle;

	[Header("Player Block")]
	[SerializeField]
	private InventoryStashPCView m_StashView;

	[SerializeField]
	private OwlcatMultiButton m_SellAllTrashButton;

	[SerializeField]
	private TextMeshProUGUI m_SellAllTrashText;

	[Header("Exchange Part")]
	[SerializeField]
	private ItemSlotsGroupView m_PlayerExchangeGroup;

	[SerializeField]
	private ItemSlotsGroupView m_VendorExchangeGroup;

	[SerializeField]
	private InventorySlotView m_VendorBarterSlotView;

	[SerializeField]
	private InventorySlotView m_PlayerBarterSlotView;

	[SerializeField]
	private OwlcatMultiButton m_VendorPriceButton;

	[SerializeField]
	private TextMeshProUGUI m_VendorPriceText;

	[SerializeField]
	private OwlcatMultiButton m_PlayerPriceButton;

	[SerializeField]
	private TextMeshProUGUI m_PlayerPriceText;

	[Header("Deal Block")]
	[SerializeField]
	private OwlcatMultiButton m_DealButton;

	[SerializeField]
	private TextMeshProUGUI m_DealLabel;

	[SerializeField]
	private OwlcatMultiButton m_BackToStashButton;

	[SerializeField]
	private TextMeshProUGUI m_BackToStashLabel;

	[SerializeField]
	private OwlcatMultiButton m_DealPriceButton;

	[SerializeField]
	private TextMeshProUGUI m_DealPrice;

	[SerializeField]
	private TextMeshProUGUI m_NotEnoughMoney;

	[SerializeField]
	private GameObject m_NotEnoughMoneyPanel;

	[SerializeField]
	private TextMeshProUGUI m_DealPricePlusArt;

	[SerializeField]
	private TextMeshProUGUI m_DealPriceMinusArt;

	[Header("Vendor reputation levels block")]
	[SerializeField]
	private GameObject m_VendorReputationBlock;

	[SerializeField]
	private TextMeshProUGUI m_VendorFearReputationLevel;

	[SerializeField]
	private TextMeshProUGUI m_VendorRespectReputationLevel;

	[SerializeField]
	private OwlcatSelectable m_FearReputationBlock;

	[SerializeField]
	private OwlcatSelectable m_RespectReputationBlock;

	[SerializeField]
	private TextMeshProUGUI m_HideUnavailableFilterLabel;

	[SerializeField]
	private OwlcatToggle m_HideUnavailableToggle;

	[Header("DiscountBlock")]
	[SerializeField]
	protected GameObject m_DiscountBlock;

	[SerializeField]
	protected TextMeshProUGUI m_DiscountText;

	[SerializeField]
	protected TextMeshProUGUI m_DiscountValue;

	[Header("No items to sell block")]
	[SerializeField]
	protected CanvasGroup m_NoItemsToSell;

	[SerializeField]
	protected TextMeshProUGUI m_NoItemsToSellText;

	[Header("Party")]
	[SerializeField]
	protected PartyPCWindowsView m_PartyView;

	[Header("Split/transition window")]
	[SerializeField]
	protected VendorTransitionWindowView m_TransitionWindowPCView;

	private IDisposable m_DealButtonHintSubscription;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_VendorGroup.Initialize(m_VendorSlotPrefab);
		m_VendorItemsFilter.Initialize();
		m_StashView.Initialize();
		m_PlayerExchangeGroup.Initialize(m_PlayerBarterSlotView);
		m_VendorExchangeGroup.Initialize(m_VendorBarterSlotView);
		m_PartyView.Initialize();
		m_DealLabel.text = UIStrings.Instance.Vendor.Deal;
		m_VendorNoPortraitNoDataText.text = UIStrings.Instance.Vendor.NoItemsVendor;
		m_HideUnavailableFilterLabel.text = UIStrings.Instance.InventoryScreen.HideUnavailableItems;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_VendorReputationBlock.gameObject.SetActive(base.ViewModel.HasFaction);
		base.ViewModel.IsPossibleDeal.Subscribe(delegate(bool value)
		{
			m_DealButton.Interactable = value;
			m_DealButtonHintSubscription?.Dispose();
			if (!value)
			{
				m_DealButtonHintSubscription = m_DealButton.SetHint(base.ViewModel.DealInactiveWarning());
			}
		}).AddTo(this);
		base.ViewModel.DealPrice.Subscribe(delegate(int value)
		{
			int num = Math.Abs(value);
			m_DealPrice.text = num.ToString();
			bool active = value > 0;
			bool active2 = value < 0;
			if (m_DealPricePlusArt != null)
			{
				m_DealPricePlusArt.gameObject.SetActive(active);
			}
			if (m_DealPriceMinusArt != null)
			{
				m_DealPriceMinusArt.gameObject.SetActive(active2);
			}
			bool active3 = value != 0;
			m_DealPriceButton.gameObject.SetActive(active3);
			if (value + base.ViewModel.PlayerMoney < 0)
			{
				m_NotEnoughMoneyPanel.SetActive(value: true);
				m_NotEnoughMoney.text = Math.Abs(value + base.ViewModel.PlayerMoney).ToString();
			}
			else
			{
				m_NotEnoughMoneyPanel.SetActive(value: false);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DealButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Deal();
		}).AddTo(this);
		base.ViewModel.VendorName.Subscribe(delegate(string value)
		{
			m_VendorName.text = value;
		}).AddTo(this);
		base.ViewModel.VendorFactionName.Subscribe(delegate(string faction)
		{
			m_FactionName.text = faction;
		}).AddTo(this);
		base.ViewModel.VendorSprite.Subscribe(SetVendorPortrait).AddTo(this);
		base.ViewModel.FactionSprite.Subscribe(SetVendorFactionImage).AddTo(this);
		base.ViewModel.VendorFaction.Subscribe(delegate(FactionType? faction)
		{
			if (faction.HasValue)
			{
				TooltipTemplateGlossary template = new TooltipTemplateGlossary(UIUtilityEncyclopedy.GetFactionEncyclopediaKey(faction.Value));
				m_FactionName.SetTooltip(template).AddTo(this);
				m_FactionIcon.SetTooltip(template).AddTo(this);
				m_FactionIconBottom.SetTooltip(template).AddTo(this);
			}
		}).AddTo(this);
		m_VendorFearReputationLevel.text = base.ViewModel.VendorFearReputationLevel.ToString();
		m_VendorRespectReputationLevel.text = base.ViewModel.VendorRespectReputationLevel.ToString();
		m_FearReputationBlock.SetTooltip(new TooltipTemplateGlossary("Presence")).AddTo(this);
		m_RespectReputationBlock.SetTooltip(new TooltipTemplateGlossary("Rapport")).AddTo(this);
		m_VendorGroup.Bind(base.ViewModel.VendorSlotsGroup);
		m_VendorItemsFilter.Bind(base.ViewModel.VendorItemsFilter);
		m_ShowTrashToggle.IsOn.Subscribe(delegate(bool isOn)
		{
			base.ViewModel.ShowTrashInVendor(isOn);
		}).AddTo(this);
		m_HideUnavailableToggle.IsOn.Subscribe(delegate(bool hide)
		{
			base.ViewModel.HideUnavailable(hide);
		}).AddTo(this);
		base.ViewModel.TransitionWindowVM.Subscribe(delegate(VendorTransitionWindowVM value)
		{
			m_TransitionWindowPCView.Bind(value);
		}).AddTo(this);
		m_StashView.Bind(base.ViewModel.StashVM);
		if (m_BackToStashButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(m_BackToStashButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.BackToStash();
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(m_SellAllTrashButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.MoveTrashForSale();
		}).AddTo(this);
		base.ViewModel.CanSellTrash.Subscribe(delegate(bool canSell)
		{
			m_SellAllTrashButton.SetInteractable(canSell);
		}).AddTo(this);
		base.ViewModel.SellTrashButtonLayer.Subscribe(delegate(int layer)
		{
			m_SellAllTrashButton.SetActiveLayer(layer);
			m_SellAllTrashText.text = ((layer == 0) ? UIStrings.Instance.Vendor.SellAllTrash : UIStrings.Instance.Vendor.ReturnTrash);
		}).AddTo(this);
		m_PlayerExchangeGroup.Bind(base.ViewModel.PlayerExchangePart);
		m_VendorExchangeGroup.Bind(base.ViewModel.VendorExchangePart);
		base.ViewModel.PlayerExchangePrice.Subscribe(delegate(int price)
		{
			m_PlayerPriceText.text = price.ToString();
			m_PlayerPriceButton.SetInteractable(price > 0);
		}).AddTo(this);
		base.ViewModel.VendorExchangePrice.Subscribe(delegate(int price)
		{
			m_VendorPriceText.text = price.ToString();
			m_VendorPriceButton.SetInteractable(price > 0);
		}).AddTo(this);
		m_DiscountBlock.SetActive(base.ViewModel.HasDiscount);
		if (base.ViewModel.HasDiscount)
		{
			m_DiscountText.text = UIStrings.Instance.Vendor.Discount;
			m_DiscountValue.text = base.ViewModel.DiscountValue.ToString();
		}
		base.ViewModel.VendorHasItemsToSell.Subscribe(SetNoItemsToSell).AddTo(this);
		m_PartyView.Bind(base.ViewModel.PartyVM);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.PlayerExchangePart.CollectionChangedCommand, delegate
		{
			m_PlayerExchangeGroup.ForceScrollToTop();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.VendorExchangePart.CollectionChangedCommand, delegate
		{
			m_VendorExchangeGroup.ForceScrollToTop();
		}).AddTo(this);
		UISounds.Instance.SetClickSound(m_DealButton, ButtonSoundsEnum.NoSound);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	private void SetNoItemsToSell(bool value)
	{
		if (m_NoItemsToSell != null && m_NoItemsToSellText != null)
		{
			m_NoItemsToSell.alpha = ((!value) ? 1 : 0);
			m_NoItemsToSellText.text = UIStrings.Instance.Tooltips.NoItemsAvailableToSelect;
		}
	}

	private void SetVendorPortrait(Sprite portrait)
	{
		m_VendorPortrait.gameObject.SetActive(portrait != null);
		m_VendorNoPortraitEffect.SetActive(portrait == null);
		if (portrait != null)
		{
			m_VendorPortrait.sprite = portrait;
		}
	}

	private void SetVendorFactionImage(Sprite factionImage)
	{
		SetFactionIcon(m_FactionIcon, factionImage);
		SetFactionIcon(m_FactionIconBottom, factionImage);
	}

	private void SetFactionIcon(Image icon, Sprite sprite)
	{
		bool flag = sprite != null;
		icon.gameObject.SetActive(flag);
		if (flag)
		{
			icon.sprite = sprite;
		}
	}
}
