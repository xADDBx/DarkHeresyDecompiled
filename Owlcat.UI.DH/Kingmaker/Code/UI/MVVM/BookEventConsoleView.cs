using System;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventConsoleView : BookEventBaseView
{
	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	private readonly ReactiveProperty<bool> m_HasGlossaryPoints = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_VotesMode = new ReactiveProperty<bool>();

	private IDisposable m_GlossaryDisposable;

	public override void Awake()
	{
		base.Awake();
		m_FirstGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
		m_SecondGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		CloseGlossary();
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
	}

	protected new void CreateInputImpl()
	{
	}

	private void AddVotesInput()
	{
	}

	private void ShowVotes()
	{
	}

	private void CloseVotes()
	{
	}

	private void SetVotesNavigation()
	{
	}

	private void ShowGlossary()
	{
	}

	private void CloseGlossary()
	{
	}

	private void CalculateGlossary()
	{
	}

	protected override void OnCloseGlossaryMode()
	{
		base.OnCloseGlossaryMode();
		CloseGlossary();
	}

	private void OnGlossaryFocusChanged(IConsoleEntity focus)
	{
		if (focus == null)
		{
			return;
		}
		RectTransform rect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
		bool num = !base.IsShowHistory.CurrentValue && !m_CuesScrollRect.IsInViewport(rect);
		Action action = delegate
		{
			if (!base.IsShowHistory.CurrentValue)
			{
				m_CuesScrollRect.SnapToCenter(rect);
			}
		};
		if (num)
		{
			action();
		}
	}

	public void OnShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	private void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_AnswersScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
