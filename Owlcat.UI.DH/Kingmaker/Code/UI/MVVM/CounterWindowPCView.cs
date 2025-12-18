using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CounterWindowPCView : View<CounterWindowVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ItemName;

	[SerializeField]
	private Image m_ItemIcon;

	[SerializeField]
	private TextMeshProUGUI m_ItemCount;

	[SerializeField]
	protected Slider m_CountSlider;

	[SerializeField]
	private TextMeshProUGUI m_CountText;

	[FormerlySerializedAs("m_SplitButton")]
	[FormerlySerializedAs("m_Button")]
	[SerializeField]
	protected OwlcatButton m_OperationButton;

	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	[SerializeField]
	protected OwlcatButton m_CloseButton;

	[SerializeField]
	private TextMeshProUGUI m_CloseButtonLabel;

	private const string ResultTextFormat = "{0}/{1}";

	protected override void OnBind()
	{
		Show();
		SetupView();
		SubscribeEvents();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void SetupView()
	{
		m_ItemName.text = base.ViewModel.ItemName;
		m_ItemIcon.sprite = base.ViewModel.ItemIcon;
		m_ItemCount.text = base.ViewModel.ItemCount;
		SetButtonLabel();
		m_CountSlider.minValue = 1f;
		m_CountSlider.maxValue = base.ViewModel.MaxValue;
		m_CountSlider.value = base.ViewModel.CurrentValue;
		SetCounterText();
	}

	private void SubscribeEvents()
	{
		m_CountSlider.OnValueChangedAsObservable().Subscribe(OnSliderValueChanged).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_OperationButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Accept();
		}).AddTo(this);
	}

	private void OnSliderValueChanged(float value)
	{
		base.ViewModel.CurrentValue = (int)value;
		SetCounterText();
	}

	private void SetCounterText()
	{
		m_CountText.text = $"{base.ViewModel.CurrentValue}/{((base.ViewModel.OperationType == CounterWindowType.Split) ? (base.ViewModel.MaxValue - base.ViewModel.CurrentValue + 1) : base.ViewModel.MaxValue)}";
	}

	private void SetButtonLabel()
	{
		m_ButtonLabel.text = GetOperationButtonText();
		m_CloseButtonLabel.text = UIStrings.Instance.CommonTexts.CloseWindow;
	}

	protected string GetOperationButtonText()
	{
		return base.ViewModel.OperationType switch
		{
			CounterWindowType.Drop => UIStrings.Instance.ActionTexts.DropItem, 
			CounterWindowType.Split => UIStrings.Instance.ActionTexts.SplitItem, 
			CounterWindowType.Move => UIStrings.Instance.ActionTexts.MoveItem, 
			_ => string.Empty, 
		};
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
