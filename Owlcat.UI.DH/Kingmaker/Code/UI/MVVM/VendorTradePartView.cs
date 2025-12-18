using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTradePartView<TItemsFilter, TVendorSlot, TVendorTransitionWindow> : View<VendorTradePartVM> where TItemsFilter : ItemsFilterBaseView where TVendorSlot : VendorLevelItemsBaseView where TVendorTransitionWindow : VendorTransitionWindowView
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected OwlcatMultiButton m_DealButton;

	[SerializeField]
	protected TextMeshProUGUI m_DealButtonTitle;

	[SerializeField]
	protected PartyPCView m_PartyView;

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
	protected VendorRandomPhrases m_VendorPhrasesList;

	[SerializeField]
	protected ScrambledTMP m_VendorPhrase;

	[SerializeField]
	protected TextMeshProUGUI FactionName;

	[Space]
	[SerializeField]
	protected TItemsFilter m_VendorItemsFilter;

	[SerializeField]
	protected TVendorSlot m_VendorSlotPrefab;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationValues;

	[SerializeField]
	protected TVendorTransitionWindow m_TransitionWindowPCView;

	[SerializeField]
	protected TextMeshProUGUI m_VendorReputationLevel;

	[SerializeField]
	protected VirtualListGridVertical m_ScrollRect;

	[Space]
	[SerializeField]
	protected GameObject m_NoItemsToSell;

	[SerializeField]
	protected TextMeshProUGUI m_NoItemsToSellText;

	[Header("DiscountBlock")]
	[SerializeField]
	protected GameObject m_DiscountBlock;

	[SerializeField]
	protected TextMeshProUGUI m_DiscountText;

	[SerializeField]
	protected TextMeshProUGUI m_DiscountValue;

	private readonly ReactiveCommand<Unit> m_OnUpdateSlots = new ReactiveCommand<Unit>();

	public Observable<Unit> OnUpdateSlots => m_OnUpdateSlots;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_VendorItemsFilter.Initialize();
		m_TransitionWindowPCView.Initialize();
		m_VendorNoPortraitNoDataText.text = UIStrings.Instance.QuesJournalTexts.NoData;
	}

	protected override void OnBind()
	{
		m_FadeAnimator.AppearAnimation();
		m_DiscountBlock.SetActive(base.ViewModel.HasDiscount);
		m_PartyView.Bind(base.ViewModel.PartyVM);
		base.ViewModel.OnSlotsUpdate.Subscribe(DrawEntities).AddTo(this);
		base.ViewModel.VendorName.Subscribe(delegate(string value)
		{
			m_VendorName.text = value;
		}).AddTo(this);
		base.ViewModel.VendorSprite.Subscribe(SetVendorPortrait).AddTo(this);
		base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate
		{
		}).AddTo(this);
		base.ViewModel.VendorReputationLevel.Subscribe(delegate(int l)
		{
			m_VendorReputationLevel.text = l.ToString();
		}).AddTo(this);
		base.ViewModel.VendorReputationProgressToNextLevel.Subscribe(delegate(int? exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.CurrentValue ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : $"{base.ViewModel.VendorCurrentReputationProgress} / {exp.ToString()}");
		}).AddTo(this);
		base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate(float exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.CurrentValue ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : (exp + " / " + base.ViewModel.VendorReputationProgressToNextLevel.CurrentValue));
		}).AddTo(this);
		base.ViewModel.TransitionWindowVM.Subscribe(delegate(VendorTransitionWindowVM val)
		{
			m_TransitionWindowPCView.Bind(val);
		}).AddTo(this);
		base.ViewModel.VendorHasItemsToSell.Subscribe(SetNoItemsToSell).AddTo(this);
		if (base.ViewModel.HasDiscount)
		{
			m_DiscountText.text = UIStrings.Instance.Vendor.Discount;
			m_DiscountValue.text = base.ViewModel.DiscountValue.ToString();
		}
		FactionName.text = base.ViewModel.VendorFactionName;
		m_DealButtonTitle.text = UIStrings.Instance.Vendor.Deal;
		SetVendorPhrase(helloWord: true);
		DrawEntities();
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
		base.gameObject.SetActive(value: false);
	}

	private void SetNoItemsToSell(bool value)
	{
		if (m_NoItemsToSell != null)
		{
			m_NoItemsToSell.SetActive(!value);
			m_NoItemsToSellText.text = UIStrings.Instance.Tooltips.NoItemsAvailableToSelect;
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EnableSlots, m_VendorSlotPrefab);
		m_OnUpdateSlots?.Execute();
	}

	protected void SetVendorPortrait(Sprite portrait)
	{
		m_VendorPortrait.gameObject.SetActive(portrait != null);
		m_VendorNoPortraitEffect.SetActive(portrait == null);
		if (portrait != null)
		{
			m_VendorPortrait.sprite = portrait;
		}
	}

	protected void SetVendorPhrase(bool helloWord)
	{
		if (helloWord)
		{
			m_VendorPhrase.SetText(string.Empty, m_VendorPhrasesList.TakePhrase(base.ViewModel.VendorFaction.CurrentValue, Game.Instance.TradeLogic.VendorEntity as BaseUnitEntity));
		}
		else
		{
			m_VendorPhrase.SetText(string.Empty, m_VendorPhrasesList.TakeFinishDealPhrase(base.ViewModel.VendorFaction.CurrentValue, Game.Instance.TradeLogic.VendorEntity as BaseUnitEntity));
		}
	}
}
