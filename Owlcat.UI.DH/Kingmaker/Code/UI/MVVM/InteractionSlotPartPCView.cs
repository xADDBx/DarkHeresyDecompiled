using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionSlotPartPCView : InteractionSlotPartView
{
	[SerializeField]
	private OwlcatMultiButton m_ClearButton;

	[SerializeField]
	private OwlcatMultiButton m_AddButton;

	[SerializeField]
	private OwlcatMultiButton m_ConfirmButton;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private InsertableLootSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private InsertableLootSlotPCView m_SlotPrefab;

	[SerializeField]
	private GameObject m_NoItemsStub;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_ConfirmText;

	[SerializeField]
	private TextMeshProUGUI m_SlotsGroupHeaderText;

	[SerializeField]
	private TextMeshProUGUI m_NoItemsText;

	private void Awake()
	{
		m_SlotsGroup.Initialize(m_SlotPrefab);
		m_ConfirmText.text = UIStrings.Instance.CommonTexts.Accept;
		m_SlotsGroupHeaderText.text = UIStrings.Instance.LootWindow.SuitableItems;
		m_NoItemsText.text = UIStrings.Instance.LootWindow.NoSuitableItems;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.ItemSlot.Subscribe(SetButtons).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ConfirmButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Confirm();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		base.ViewModel.RequestItems();
		base.ViewModel.SlotsGroup.Subscribe(UpdateSlotsGroup).AddTo(this);
	}

	private void SetButtons(ItemSlotVM itemVm)
	{
		bool active = itemVm != null;
		m_SlotView.gameObject.SetActive(active);
	}

	private void UpdateSlotsGroup(InsertableLootSlotsGroupVM group)
	{
		m_SlotsGroup.Bind(group);
		if (group.ValidItems.Count == 0 && base.ViewModel.Group.Items.Count == 0)
		{
			m_NoItemsStub.SetActive(value: true);
			m_SlotsGroup.gameObject.SetActive(value: false);
		}
		else
		{
			m_NoItemsStub.SetActive(value: false);
			m_SlotsGroup.gameObject.SetActive(value: true);
		}
	}
}
