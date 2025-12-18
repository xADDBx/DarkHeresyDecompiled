using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InspectConsoleView : View<InspectVM>, ITurnBasedModeHandler, ISubscriber
{
	[SerializeField]
	private InfoWindowConsoleView m_InfoWindow;

	[SerializeField]
	private RectTransform InspectRectTransform;

	[SerializeField]
	private float m_MoveAnimationTime = 0.2f;

	[SerializeField]
	private CanvasTransformSettings m_StartPosition;

	[SerializeField]
	private CanvasTransformSettings m_SidePosition;

	private InfoWindowVM m_InfoWindowVM;

	private readonly List<Tweener> m_StartedTweeners = new List<Tweener>();

	public void Initialize()
	{
		MoveContainer(InspectRectTransform, m_StartPosition, animated: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.Tooltip.Skip(1).Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_InfoWindowVM?.Dispose();
			if (value == null)
			{
				m_InfoWindow.Hide();
				MoveContainer(InspectRectTransform, m_StartPosition, animated: true);
			}
			else
			{
				m_InfoWindowVM = new InfoWindowVM(value, Close);
				m_InfoWindow.Bind(m_InfoWindowVM);
				MoveContainer(InspectRectTransform, m_SidePosition, animated: true);
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		m_StartedTweeners.Clear();
		m_InfoWindowVM?.Dispose();
		m_InfoWindowVM = null;
	}

	private void Close()
	{
		m_InfoWindow.Hide();
		Game.Instance.Player.UISettings.ShowInspect = false;
	}

	protected void MoveContainer(RectTransform rectTransform, CanvasTransformSettings settings, bool animated)
	{
		if (!(rectTransform == null))
		{
			if (animated)
			{
				m_StartedTweeners.Add(rectTransform.DORotateQuaternion(Quaternion.Euler(settings.Rotation), m_MoveAnimationTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
				m_StartedTweeners.Add(rectTransform.DOAnchorPos(settings.LocalPosition, m_MoveAnimationTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
				m_StartedTweeners.Add(rectTransform.DOScale(settings.LocalScale, m_MoveAnimationTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
			}
			else
			{
				rectTransform.rotation = Quaternion.Euler(settings.Rotation);
				rectTransform.anchoredPosition = settings.LocalPosition;
				rectTransform.localScale = settings.LocalScale;
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		Close();
	}
}
