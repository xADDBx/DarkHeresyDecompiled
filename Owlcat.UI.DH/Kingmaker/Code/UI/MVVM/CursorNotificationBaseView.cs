using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CursorNotificationBaseView : View<CursorNotificationVM>
{
	private enum ResizeMethod : byte
	{
		None,
		Width,
		Height
	}

	[SerializeField]
	private RectTransform m_Transform;

	[SerializeField]
	private RectTransform m_BackgroundTransform;

	[SerializeField]
	private TMP_Text m_NotificationText;

	[SerializeField]
	private FadeAnimator m_Fade;

	[Space]
	[SerializeField]
	private Vector2 m_Padding;

	[SerializeField]
	private Vector2 m_MinSize;

	[SerializeField]
	private Vector2 m_CursorWithTextOffset;

	[Space]
	[SerializeField]
	private ResizeMethod m_ResizeMethod;

	[SerializeField]
	private Vector2Int m_ReferenceSize = new Vector2Int(1920, 1080);

	private IDisposable m_HideCountdown;

	private GameObject m_NotificationGameObject;

	private Vector2? m_InitialSize;

	protected override void OnBind()
	{
		m_Fade.DisappearInstant();
		if (!m_NotificationGameObject)
		{
			m_NotificationGameObject = m_Transform.gameObject;
		}
		m_NotificationGameObject.SetActive(value: false);
		base.ViewModel.ShowNotification.Subscribe(Show).AddTo(this);
		Resize();
	}

	protected override void OnUnbind()
	{
		m_HideCountdown?.Dispose();
		m_HideCountdown = null;
		m_NotificationGameObject.SetActive(value: false);
	}

	protected Vector2 GetPositionOffset()
	{
		if (!base.ViewModel.IsCursorHasText())
		{
			return Vector2.zero;
		}
		return m_CursorWithTextOffset;
	}

	protected abstract Vector2 GetCursorPosition();

	private void Show((string text, TimeSpan duration) @params)
	{
		if (m_HideCountdown != null)
		{
			m_HideCountdown?.Dispose();
			m_Fade.DisappearInstant();
		}
		m_NotificationGameObject.SetActive(value: true);
		m_NotificationText.SetText(@params.text);
		m_NotificationText.ForceMeshUpdate();
		float b = m_NotificationText.preferredWidth + m_Padding.x;
		float b2 = m_NotificationText.preferredHeight + m_Padding.y;
		float x = Mathf.Max(m_MinSize.x, b);
		float y = Mathf.Max(m_MinSize.y, b2);
		m_BackgroundTransform.sizeDelta = new Vector2(x, y);
		m_Fade.AppearAnimation();
		m_HideCountdown = ObservableSubscribeExtensions.Subscribe(Observable.Timer(@params.duration), delegate
		{
			m_Fade.DisappearAnimation(delegate
			{
				m_NotificationGameObject.SetActive(value: false);
			});
			m_HideCountdown?.Dispose();
			m_HideCountdown = null;
		});
	}

	private void Resize()
	{
		ResizeMethod resizeMethod = m_ResizeMethod;
		if (resizeMethod == ResizeMethod.Width || resizeMethod == ResizeMethod.Height)
		{
			Vector2 valueOrDefault = m_InitialSize.GetValueOrDefault();
			if (!m_InitialSize.HasValue)
			{
				valueOrDefault = m_Transform.sizeDelta;
				m_InitialSize = valueOrDefault;
			}
			int num = ((m_ResizeMethod == ResizeMethod.Width) ? m_ReferenceSize.x : m_ReferenceSize.y);
			int num2 = ((m_ResizeMethod == ResizeMethod.Width) ? Screen.width : Screen.height);
			float num3 = (float)num / (float)num2;
			m_Transform.sizeDelta = m_InitialSize.Value * num3;
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		Resize();
	}

	private void Update()
	{
		if ((bool)m_NotificationGameObject && m_NotificationGameObject.activeSelf)
		{
			m_Transform.localPosition = GetCursorPosition();
		}
	}
}
