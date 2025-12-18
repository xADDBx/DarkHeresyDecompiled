using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class CharacterVisualSettingsEntityView : View<CharacterVisualSettingsEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	private bool m_IsInit;

	private LocalizedString m_LocalizedLabel;

	public void Initialize(LocalizedString label)
	{
		if (!m_IsInit)
		{
			m_LocalizedLabel = label;
			base.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_Label.text = m_LocalizedLabel.Text;
		base.gameObject.SetActive(value: true);
		base.ViewModel.Locked.Subscribe(delegate(bool value)
		{
			m_Button.Interactable = !value;
		}).AddTo(this);
		base.ViewModel.IsOn.Subscribe(delegate(bool value)
		{
			m_Button.SetActiveLayer((!value) ? 1 : 0);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
