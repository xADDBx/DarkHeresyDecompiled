using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.Components.Text;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public abstract class MessageBoxBaseView : View<MessageBoxVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MessageText;

	[SerializeField]
	private TMPLinkHandler m_LinkHandler;

	[Header("Progress Bar")]
	[SerializeField]
	private RectTransform m_ProgressParent;

	[SerializeField]
	private RectTransform m_ProgressTransform;

	[SerializeField]
	private TextMeshProUGUI m_PercentText;

	[Header("ScrollBar")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("ResetAnimation")]
	[SerializeField]
	private List<CanvasGroup> m_CanvasesResetAnimation;

	private Tweener m_ProgressTweener;

	public virtual void Awake()
	{
		ResetCanvasesAnimation();
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		m_MessageText.text = base.ViewModel.MessageText;
		ScrollToTop();
		base.ViewModel.WaitTime.Subscribe(delegate(int value)
		{
			StringBuilder stringBuilder = new StringBuilder(base.ViewModel.AcceptText);
			if (value > 0)
			{
				stringBuilder.Append($" ({value})");
			}
			SetAcceptText(stringBuilder.ToString());
		}).AddTo(this);
		SetDeclineText(base.ViewModel.DeclineText);
		base.ViewModel.InputText.CombineLatest(base.ViewModel.WaitTime, delegate(string s, int i)
		{
			bool flag = i == 0;
			if (flag && base.ViewModel.BoxType == DialogMessageBoxType.TextField)
			{
				flag = base.ViewModel.InputText.CurrentValue.Length > 0;
			}
			return flag;
		}).Subscribe(SetAcceptInteractable).AddTo(this);
		m_LinkHandler.OnClickAsObservable().Subscribe(delegate((PointerEventData Arg0, TMP_LinkInfo Arg1) value)
		{
			base.ViewModel.OnLinkInvoke(value.Arg1);
		}).AddTo(this);
		BindTextField();
		SetProgressBar();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.ViewModel.IsProgressBar.CurrentValue)
			{
				base.ViewModel.OnDeclinePressed();
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		DestroyTextField();
		m_ProgressTweener?.Kill();
		m_ProgressTweener = null;
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		ResetCanvasesAnimation();
		base.gameObject.SetActive(value: false);
	}

	private void SetProgressBar()
	{
		m_ProgressParent.gameObject.SetActive(base.ViewModel.IsProgressBar.CurrentValue);
		m_ProgressTransform.sizeDelta = new Vector2(0f, m_ProgressTransform.rect.height);
		m_PercentText.text = UIUtilityText.AddPercentTo(Mathf.CeilToInt(0f));
		if (!base.ViewModel.IsProgressBar.CurrentValue)
		{
			return;
		}
		BindProgressBar();
		if (base.ViewModel.LoadingProgress != null)
		{
			ObservableSubscribeExtensions.Subscribe(base.ViewModel.LoadingProgressCloseTrigger, delegate
			{
				base.ViewModel.OnAcceptPressed();
			}).AddTo(this);
			base.ViewModel.LoadingProgress.Subscribe(SetLoadingProgress).AddTo(this);
		}
	}

	private void SetLoadingProgress(float virtualProgress)
	{
		virtualProgress = Mathf.Clamp01(virtualProgress);
		float progressWidth = m_ProgressParent.rect.width;
		float startValue = ((m_ProgressTransform.rect.width > 0f && progressWidth > 0f) ? (m_ProgressTransform.rect.width / progressWidth) : 0f);
		m_ProgressTweener?.Kill();
		m_ProgressTweener = DOTween.To(delegate
		{
			m_ProgressTransform.sizeDelta = new Vector2(virtualProgress * progressWidth, m_ProgressTransform.rect.height);
			m_PercentText.text = UIUtilityText.AddPercentTo(Mathf.CeilToInt(virtualProgress * 100f));
		}, startValue, virtualProgress, 0.5f).SetEase(Ease.Linear);
	}

	private void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	protected virtual void OnTextInputChanged(string value)
	{
		base.ViewModel.SetInputText(value);
	}

	private void ResetCanvasesAnimation()
	{
		if (!m_CanvasesResetAnimation.Any())
		{
			return;
		}
		m_CanvasesResetAnimation.ForEach(delegate(CanvasGroup canvasGroup)
		{
			if (!(canvasGroup == null))
			{
				canvasGroup.alpha = 0f;
			}
		});
	}

	protected abstract void SetAcceptInteractable(bool interactable);

	protected abstract void SetAcceptText(string acceptText);

	protected abstract void SetDeclineText(string declineText);

	protected abstract void BindTextField();

	protected abstract void DestroyTextField();

	protected abstract void BindProgressBar();
}
