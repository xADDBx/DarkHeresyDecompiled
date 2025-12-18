using System;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class SystemNotificationView : View<SystemNotificationVM>
{
	[Header("Elements")]
	[SerializeField]
	private LayoutGroup m_Group;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private RectTransform m_Icon;

	[SerializeField]
	private Image m_Attention;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Values")]
	[SerializeField]
	private float m_MoveTime = 0.5f;

	[SerializeField]
	private float m_FadeStayOnTheScreen = 2f;

	[SerializeField]
	private Ease m_MoveEase;

	[Header("Values/Refresh")]
	[Range(0f, 1f)]
	[SerializeField]
	private float m_RefreshAlphaFrom;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_RefreshMovePercent;

	[SerializeField]
	private float m_RefreshMoveTime;

	private Tweener m_MoveTweener;

	private RectTransform m_RectTransform;

	private RectOffset m_Padding;

	private IDisposable m_HideTimer;

	public bool IsHiding { get; private set; }

	protected override void OnBind()
	{
		m_Label.SetText(base.ViewModel.Label);
		Show();
		SetupHideTimer();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		DOTween.Kill(m_MoveTweener);
		DOTween.Kill(m_Attention);
		DOTween.Kill(m_Icon);
		SetFadeToAttention(0f);
		m_Icon.localRotation = Quaternion.identity;
	}

	private void Show()
	{
		IsHiding = false;
		m_Padding = m_Group.padding;
		m_RectTransform = GetComponent<RectTransform>();
		m_Padding.left = -(int)m_RectTransform.rect.width;
		m_Padding.bottom = 0;
		m_Group.padding = m_Padding;
		DoPaddingFromTo(0f - m_RectTransform.rect.width, 0f, 0f, 0f, m_MoveTime);
		m_FadeAnimator.AppearAnimation();
	}

	public void Hide()
	{
		IsHiding = true;
		m_HideTimer?.Dispose();
		DoPaddingFromTo(0f, 0f - m_RectTransform.rect.width, 0f, 0f - m_RectTransform.rect.height, m_MoveTime);
		SetFadeToAttention(0f);
		m_FadeAnimator.DisappearAnimation(delegate
		{
			WidgetFactory.DisposeWidget(this);
		});
	}

	private void SetupHideTimer()
	{
		m_HideTimer = ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(m_FadeStayOnTheScreen), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			Hide();
		}).AddTo(this);
	}

	public void Refresh()
	{
		m_HideTimer?.Dispose();
		SetupHideTimer();
		DOTween.Kill(m_Attention);
		SetFadeToAttention(0f);
		m_Attention.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_Attention.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true).SetDelay(1f);
		});
		DOTween.Kill(m_Icon);
		m_Icon.localRotation = Quaternion.identity;
		m_Icon.DOShakeRotation(0.3f, new Vector3(10f, 10f, 30f), 23, 54f).SetUpdate(isIndependentUpdate: true).SetLoops(5, LoopType.Yoyo);
		m_FadeAnimator.CanvasGroup.alpha = m_RefreshAlphaFrom;
		m_FadeAnimator.AppearAnimation();
		DoPaddingFromTo((0f - m_RectTransform.rect.width) * (1f - m_RefreshMovePercent), 0f, 0f, 0f, m_RefreshMoveTime);
	}

	private void DoPaddingFromTo(float fromLeft, float toLeft, float fromBottom, float toBottom, float time)
	{
		m_MoveTweener?.Kill();
		m_MoveTweener = DOTween.To(() => 0f, delegate(float x)
		{
			m_Padding.left = (int)Mathf.Lerp(fromLeft, toLeft, x);
			m_Padding.bottom = (int)Mathf.Lerp(fromBottom, toBottom, x);
			m_Group.padding = m_Padding;
		}, 1f, time).OnUpdate(delegate
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);
		}).SetUpdate(isIndependentUpdate: true)
			.SetEase(m_MoveEase);
	}

	private void SetFadeToAttention(float alpha)
	{
		Color color = m_Attention.color;
		color.a = alpha;
		m_Attention.color = color;
	}
}
