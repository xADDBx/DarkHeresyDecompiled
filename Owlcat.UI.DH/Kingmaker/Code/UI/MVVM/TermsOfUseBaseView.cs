using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TermsOfUseBaseView : View<TermsOfUseVM>
{
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_MainContainer;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Licence;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_SubLicence;

	[SerializeField]
	[UsedImplicitly]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private bool m_IsShowed;

	public void Initialize()
	{
		PFLog.System.Log("Initializing terms of use window...");
		m_MainContainer.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Title.text = base.ViewModel.TermsOfUseTexts.Header;
		m_Licence.text = base.ViewModel.GetLicenceText();
		m_SubLicence.text = base.ViewModel.TermsOfUseTexts.SubLicence;
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			m_MainContainer.AppearAnimation();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(ServiceWindowsType.LocalMap);
		}
	}

	public void Hide()
	{
		if (m_IsShowed)
		{
			m_MainContainer.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(ServiceWindowsType.LocalMap);
		}
	}
}
