using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class TutorialWindowBaseView<TViewModel> : View<TViewModel>, ISettingsFontSizeUIHandler, ISubscriber where TViewModel : TutorialWindowVM
{
	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[Space]
	[SerializeField]
	private GameObject m_ImageContainer;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private VideoPlayerHelper m_VideoPlayerHelper;

	[SerializeField]
	private Sprite m_DefaultSprite;

	[Space]
	[SerializeField]
	protected TextMeshProUGUI m_TriggerText;

	[SerializeField]
	protected TextMeshProUGUI m_TutorialText;

	[SerializeField]
	protected TextMeshProUGUI m_SolutionText;

	[Space]
	[SerializeField]
	protected OwlcatToggle m_DontShowToggle;

	[SerializeField]
	protected TextMeshProUGUI m_DontShowLabel;

	[Space]
	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	private ScrollRectExtended m_BodyScrollRect;

	[SerializeField]
	private RectTransform m_BodyContentRectTransform;

	protected virtual bool IsShowDefaultSprite => false;

	protected virtual bool IsBigTutorial => true;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.Initialize();
		if (m_VideoPlayerHelper != null)
		{
			m_VideoPlayerHelper.Initialize();
		}
		Show();
		m_TriggerText.SetLinkTooltip().AddTo(this);
		m_TutorialText.SetLinkTooltip().AddTo(this);
		m_SolutionText.SetLinkTooltip().AddTo(this);
		UITutorial tutorial = UIStrings.Instance.Tutorial;
		m_DontShowToggle.gameObject.SetActive(base.ViewModel.CanBeBanned);
		m_DontShowToggle.Set(value: false);
		UISounds.Instance.SetClickAndHoverSound(m_DontShowToggle.ConsoleEntityProxy as OwlcatMultiButton, ButtonSoundsEnum.NoSound);
		if (base.ViewModel.CanBeBanned && !(m_DontShowLabel == null))
		{
			if (base.ViewModel.BanTutorInsteadOfTag)
			{
				m_DontShowLabel.text = tutorial.DontShowThisTutorial.Text;
			}
			else
			{
				m_DontShowLabel.text = string.Format(tutorial.DontShowTutorialTag.Text, (!base.ViewModel.TutorialTag.HasValue) ? null : tutorial.TagNames.GetTagName(base.ViewModel.TutorialTag.Value)?.Text);
			}
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		OnHide();
		if (m_DontShowToggle.IsOn.CurrentValue)
		{
			base.ViewModel.BanTutor();
		}
		m_DontShowToggle.Set(value: false);
		SetActive(show: false);
		foreach (TutorialData.Page page in base.ViewModel.Data.Pages)
		{
			page.Picture?.ForceUnload();
			page.Video?.ForceUnload();
		}
	}

	public virtual void Show()
	{
		m_WindowAnimator.AppearAnimation();
		OnShow();
	}

	public void SetActive(bool show)
	{
		base.gameObject.SetActive(show);
		SetPage(null);
	}

	protected virtual void OnShow()
	{
		EscHotkeyManager.Instance.Subscribe(OnEscPressed).AddTo(this);
	}

	protected virtual void OnHide()
	{
		EscHotkeyManager.Instance.Unsubscribe(OnEscPressed);
	}

	private void OnEscPressed()
	{
		base.ViewModel.Hide();
	}

	protected void SetPage(TutorialData.Page page)
	{
		TViewModel viewModel = base.ViewModel;
		if (viewModel != null)
		{
			_ = viewModel.FontSizeMultiplier;
			if (true)
			{
				SetTextsSize(base.ViewModel.FontSizeMultiplier);
			}
		}
		m_Title.text = page?.Title;
		m_ImageContainer.gameObject.SetActive((page?.Picture != null && page.Picture.Exists()) || (page?.Video != null && page.Video.Exists()) || m_DefaultSprite != null);
		if (IsShowDefaultSprite)
		{
			m_Image.gameObject.SetActive(page != null && (page.Video == null || !page.Video.Exists()));
			if (page != null && (page.Video == null || !page.Video.Exists()))
			{
				m_Image.sprite = ((page.Picture != null && page.Picture.Exists()) ? page.Picture.Load() : m_DefaultSprite);
			}
		}
		else
		{
			m_Image.gameObject.SetActive(page?.Picture != null && page.Picture.Exists());
			if (page?.Picture != null)
			{
				m_Image.sprite = page.Picture.Load();
			}
		}
		bool valueOrDefault = (page?.Video?.Exists()).GetValueOrDefault();
		if (m_VideoPlayerHelper != null)
		{
			m_VideoPlayerHelper.gameObject.SetActive(valueOrDefault);
			if (valueOrDefault)
			{
				m_VideoPlayerHelper.SetClip(page.Video.Load(), SoundStateType.Video, prepareVideo: false, null, null);
			}
		}
		m_TriggerText.text = page?.TriggerText;
		m_TriggerText.gameObject.SetActive(!string.IsNullOrEmpty(page?.TriggerText));
		m_TutorialText.text = page?.Description;
		m_TutorialText.gameObject.SetActive(!string.IsNullOrEmpty(page?.Description));
		m_SolutionText.text = page?.SolutionText;
		m_SolutionText.gameObject.SetActive(!string.IsNullOrEmpty(page?.SolutionText));
	}

	protected virtual void SetTextsSize(float multiplier)
	{
		if (m_BodyContentRectTransform != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_BodyContentRectTransform);
		}
		m_BodyScrollRect.Or(null)?.ScrollToTop();
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextsSize(size);
	}
}
