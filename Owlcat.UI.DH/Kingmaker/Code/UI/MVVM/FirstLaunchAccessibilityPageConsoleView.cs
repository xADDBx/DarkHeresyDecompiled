using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchAccessibilityPageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchAccessiabilityPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderProtanopiaConsoleView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderDeuteranopiaConsoleView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderTritanopiaConsoleView;

	protected override void OnBind()
	{
		m_SettingsEntitySliderProtanopiaConsoleView.Bind(base.ViewModel.Protanopia);
		m_SettingsEntitySliderDeuteranopiaConsoleView.Bind(base.ViewModel.Deuteranopia);
		m_SettingsEntitySliderTritanopiaConsoleView.Bind(base.ViewModel.Tritanopia);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.OnBind();
	}

	public void ScrollDescription(float x)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Scroll(x);
		}
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_InfoView.ScrollRectExtended.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}
}
