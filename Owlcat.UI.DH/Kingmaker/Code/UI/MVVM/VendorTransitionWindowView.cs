using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTransitionWindowView : View<VendorTransitionWindowVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[SerializeField]
	private InventorySlotView m_Slot;

	[SerializeField]
	private GameObject m_SliderBlock;

	[SerializeField]
	protected Slider m_Slider;

	[SerializeField]
	private TextMeshProUGUI m_SliderText;

	private const string ResultTextFormat = "{0}/{1}";

	public void Initialize()
	{
	}

	protected override void OnBind()
	{
		m_Slot.Bind(base.ViewModel.Slot);
		base.gameObject.SetActive(value: true);
		m_SliderBlock.SetActive(base.ViewModel.MaxValue > 1);
		m_Header.text = UIStrings.Instance.Vendor.ProceedTransaction.Text;
		m_Slider.minValue = 1f;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = base.ViewModel.CurrentValue;
		m_SliderText.text = $"{base.ViewModel.MaxValue}/{base.ViewModel.MaxValue}";
		m_Slider.OnValueChangedAsObservable().Subscribe(OnSliderValueChanged).AddTo(this);
		ModalWindowsSounds.Instance.MessageBox.Show.Play();
	}

	protected override void OnUnbind()
	{
		ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		base.gameObject.SetActive(value: false);
	}

	protected void Deal()
	{
		base.ViewModel.Deal();
	}

	protected void Close()
	{
		base.ViewModel.Close();
	}

	private void OnSliderValueChanged(float value)
	{
		base.ViewModel.CurrentValue = (int)value;
		SetCounterText();
	}

	private void SetCounterText()
	{
		m_SliderText.text = $"{base.ViewModel.CurrentValue}/{base.ViewModel.MaxValue}";
	}
}
