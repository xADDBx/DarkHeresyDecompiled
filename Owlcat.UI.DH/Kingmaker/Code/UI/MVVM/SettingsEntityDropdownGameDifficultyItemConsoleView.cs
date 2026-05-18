using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityDropdownGameDifficultyItemConsoleView : View<SettingsEntityDropdownGameDifficultyItemVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_FocusCanvas;

	protected override void OnBind()
	{
		if (m_Icon != null)
		{
			m_Icon.sprite = base.ViewModel.Icon;
		}
		m_Title.text = base.ViewModel.Title;
		base.ViewModel.IsSelected.Subscribe(SetValueFromSettings).AddTo(this);
	}

	private void SetValueFromSettings(bool value)
	{
		m_Button.SetActiveLayer(value ? "On" : "Off");
		if (m_FocusCanvas != null)
		{
			m_FocusCanvas.alpha = (value ? 1 : 0);
		}
		if (value)
		{
			ButtonsSounds.Instance.Default.Hover.Play();
		}
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, base.ViewModel.Title, base.ViewModel.Description);
			});
		}
	}
}
