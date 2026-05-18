using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class HintView : View<HintVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private string m_BindingTextFormat = " ({0})";

	private Dictionary<string, string> m_TextTags = new Dictionary<string, string> { { "<separator>", "<align=\"center\">------------------</align>" } };

	private CanvasGroup m_CanvasGroup;

	private Tweener m_ShowTween;

	private RectTransform m_RectTransform;

	private RectTransform m_ParentRectTransform;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>();
		m_RectTransform = (RectTransform)base.transform;
		m_ParentRectTransform = (RectTransform)base.transform.parent;
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetHintText();
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			UpdateHintPosition();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(0.20000000298023224), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			DelayedBind();
		}).AddTo(this);
	}

	private void DelayedBind()
	{
		base.gameObject.SetActive(value: true);
		m_CanvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(0.10000000149011612), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				UpdateHintPosition();
				m_ShowTween = m_CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					SystemSounds.Instance.Hint.Show.Play();
				}).SetUpdate(isIndependentUpdate: true);
			}
		}).AddTo(this);
	}

	private void SetHintText()
	{
		StringBuilder stringBuilder = new StringBuilder(base.ViewModel.Text);
		if (!string.IsNullOrEmpty(base.ViewModel.BindingText))
		{
			stringBuilder.Append(string.Format(m_BindingTextFormat, base.ViewModel.BindingText));
		}
		ApplyTextTags(stringBuilder);
		m_Label.text = stringBuilder.ToString();
		m_Label.color = base.ViewModel.Color;
	}

	private void ApplyTextTags(StringBuilder sb)
	{
		foreach (KeyValuePair<string, string> textTag in m_TextTags)
		{
			sb.Replace(textTag.Key, textTag.Value);
		}
	}

	private void UpdateHintPosition()
	{
		if (m_ParentRectTransform == null)
		{
			m_ParentRectTransform = (RectTransform)base.transform.parent;
		}
		Vector2 cursorPosition = CursorController.CursorPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, cursorPosition, UICamera.Instance, out var localPoint);
		m_RectTransform.localPosition = UIUtilityRect.LimitPositionRectInRect(localPoint, m_ParentRectTransform, m_RectTransform);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		if (Mathf.Approximately(m_CanvasGroup.alpha, 1f))
		{
			SystemSounds.Instance.Hint.Hide.Play();
		}
		m_CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		StopAllCoroutines();
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}
}
