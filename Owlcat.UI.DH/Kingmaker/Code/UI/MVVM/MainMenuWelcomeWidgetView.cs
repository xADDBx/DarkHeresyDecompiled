using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuWelcomeWidgetView : View<MainMenuWelcomeWidgetVM>
{
	[Header("Buttons")]
	[SerializeField]
	protected OwlcatButton m_LicenceButton;

	[SerializeField]
	protected TextMeshProUGUI m_LicenceLabel;

	[SerializeField]
	protected OwlcatButton m_WebsiteButton;

	[SerializeField]
	protected TextMeshProUGUI m_WebsiteLabel;

	[SerializeField]
	protected OwlcatButton m_DiscordButton;

	[SerializeField]
	protected TextMeshProUGUI m_DiscordLabel;

	[Header("WelcomeText")]
	[SerializeField]
	protected TextMeshProUGUI m_IntroductoryText;

	[SerializeField]
	protected GameObject m_WelcomeTextContainer;

	[SerializeField]
	protected FadeAnimator m_WelcomeTextBlock;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected float m_DelayBeforeShow = 3f;

	public void Awake()
	{
		m_WelcomeTextContainer.SetActive(value: false);
		m_WelcomeTextBlock.Initialize();
	}

	protected override void OnBind()
	{
		m_DiscordLabel.Or(null)?.gameObject.SetActive(value: true);
		m_WebsiteButton.Or(null)?.gameObject.SetActive(value: true);
		m_DiscordLabel.Or(null)?.gameObject.SetActive(value: true);
		SetTextMessageOfTheDay();
		ObservableSubscribeExtensions.Subscribe(m_WebsiteButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenUrl(FeedbackPopupItemType.Website);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LicenceButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowLicense();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DiscordButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenUrl(FeedbackPopupItemType.Discord);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateMessageOfTheDay();
		}).AddTo(this);
		m_WelcomeTextContainer.SetActive(value: true);
		DelayedInvoker.InvokeInTime(delegate
		{
			m_WelcomeTextBlock.AppearAnimation();
			FullScreenUniqueSounds.Instance.MainMenu.MessageOfTheDayShow.Play();
		}, m_DelayBeforeShow);
	}

	private void SetTextMessageOfTheDay()
	{
		m_WebsiteLabel.text = UIStrings.Instance.FeedbackPopupTexts.GetTitleByPopupItemType(FeedbackPopupItemType.Website);
		m_LicenceLabel.text = UIStrings.Instance.MainMenu.License;
		m_DiscordLabel.text = UIStrings.Instance.FeedbackPopupTexts.Discord;
		base.ViewModel.GetIntroductoryText(delegate(string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_IntroductoryText.text = text;
			}
		});
		UtilityLink.SetTextLink(m_IntroductoryText).AddTo(this);
	}

	protected virtual void UpdateMessageOfTheDay()
	{
		SetTextMessageOfTheDay();
	}
}
