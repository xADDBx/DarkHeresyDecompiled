using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CursorNotificationVM : ViewModel, ICursorNotificationUIHandler, ISubscriber
{
	private const int DurationMs = 2500;

	private readonly TimeSpan m_Duration;

	private readonly ReactiveCommand<(string text, TimeSpan duration)> m_ShowNotification;

	private readonly Func<Vector3> m_GetCursorRawPosition;

	private readonly Func<bool> m_CursorHasText;

	public Observable<(string text, TimeSpan duration)> ShowNotification => m_ShowNotification;

	public CursorNotificationVM(Func<Vector3> getCursorRawPosition, Func<bool> cursorHasText)
	{
		m_GetCursorRawPosition = getCursorRawPosition;
		m_CursorHasText = cursorHasText;
		m_Duration = TimeSpan.FromMilliseconds(2500.0);
		m_ShowNotification = new ReactiveCommand<(string, TimeSpan)>().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public Vector3 GetCursorRawPosition()
	{
		return m_GetCursorRawPosition();
	}

	public bool IsCursorHasText()
	{
		return m_CursorHasText();
	}

	void ICursorNotificationUIHandler.HandleNotification(string text, WarningNotificationFormat format)
	{
		m_ShowNotification.Execute((text, m_Duration));
		UISounds.Instance.Sounds.Combat.CursorNotificationMessage.Play();
	}
}
