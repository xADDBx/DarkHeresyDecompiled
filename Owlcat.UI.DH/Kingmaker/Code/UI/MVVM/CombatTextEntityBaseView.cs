using System;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatTextEntityBaseView<TCombatMessage> : MonoBehaviour where TCombatMessage : CombatMessageBase
{
	[SerializeField]
	public float Duration = 3f;

	[SerializeField]
	protected float ShowFadeTime = 0.3f;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private CanvasGroup[] m_fadeOnAwake;

	private TimeSpan m_TargetTime;

	private Action m_EndCallback;

	private bool m_DisposeSoon;

	protected CanvasGroup CanvasGroup => m_CanvasGroup;

	public RectTransform Rect => base.transform as RectTransform;

	public Vector2 Size => Rect.sizeDelta;

	public float XPos => GetXPos();

	public void SetData(TCombatMessage combatMessage, Action endCallback = null)
	{
		m_DisposeSoon = false;
		DoData(combatMessage);
		DoShow();
		RenewTimer();
		m_EndCallback = endCallback;
	}

	public void RenewTimer()
	{
		if (!m_DisposeSoon)
		{
			m_TargetTime = Game.Instance.Controllers.TimeController.RealTime + Duration.Seconds();
		}
	}

	public virtual void Dispose()
	{
		m_EndCallback?.Invoke();
		m_EndCallback = null;
	}

	protected abstract float GetXPos();

	protected abstract void DoData(TCombatMessage combatMessage);

	protected virtual void DoShow()
	{
		if (Game.Instance.IsPaused)
		{
			CanvasGroup.alpha = 1f;
			return;
		}
		CanvasGroup.alpha = 0f;
		CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
	}

	[UsedImplicitly]
	private void Update()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess && !m_DisposeSoon && m_TargetTime <= Game.Instance.Controllers.TimeController.RealTime)
		{
			CanvasGroup.DOFade(0f, ShowFadeTime).OnComplete(Dispose).SetUpdate(isIndependentUpdate: true);
			m_DisposeSoon = true;
		}
	}

	private void Awake()
	{
		CanvasGroup[] fadeOnAwake = m_fadeOnAwake;
		for (int i = 0; i < fadeOnAwake.Length; i++)
		{
			fadeOnAwake[i].alpha = 0f;
		}
	}
}
