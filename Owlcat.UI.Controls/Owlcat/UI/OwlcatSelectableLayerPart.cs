using System;
using Owlcat.UI.Tweenable;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.UI;

[Serializable]
public class OwlcatSelectableLayerPart
{
	public class CanvasFader
	{
		private CanvasGroup m_CanvasGroup;

		private float m_StartAlpha;

		private float m_TargetAlpha;

		private float m_FadeDuration;

		private float m_ElapsedTime;

		private bool m_IsFading;

		private IDisposable m_FadeAnimation;

		private Action m_DisposeCallback;

		public CanvasFader(CanvasGroup canvasGroup, Action disposeCallback)
		{
			m_CanvasGroup = canvasGroup;
			m_DisposeCallback = disposeCallback;
		}

		public void StartFade(float endAlpha, float duration)
		{
			if (m_CanvasGroup == null)
			{
				Dispose();
				return;
			}
			if (float.IsNaN(m_CanvasGroup.alpha))
			{
				m_CanvasGroup.alpha = 0f;
				Dispose();
				return;
			}
			m_StartAlpha = m_CanvasGroup.alpha;
			m_TargetAlpha = endAlpha;
			m_FadeDuration = duration;
			m_ElapsedTime = 0f;
			m_IsFading = true;
			m_FadeAnimation = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate), delegate
			{
				UpdateFade(Time.unscaledDeltaTime);
			});
		}

		private void UpdateFade(float deltaTime)
		{
			if (!m_IsFading)
			{
				return;
			}
			if (m_CanvasGroup == null)
			{
				Dispose();
				return;
			}
			m_ElapsedTime += deltaTime;
			float t = Mathf.Clamp01(m_ElapsedTime / m_FadeDuration);
			m_CanvasGroup.alpha = Mathf.Lerp(m_StartAlpha, m_TargetAlpha, t);
			if (Mathf.Approximately(m_CanvasGroup.alpha, m_TargetAlpha))
			{
				StopFading();
			}
		}

		private void StopFading()
		{
			m_FadeAnimation.Dispose();
			m_FadeAnimation = null;
			m_IsFading = false;
		}

		private void Dispose()
		{
			StopFading();
			m_DisposeCallback?.Invoke();
		}
	}

	[SerializeField]
	private Graphic m_TargetGraphic;

	[SerializeField]
	private GameObject m_TargetGameObject;

	[SerializeField]
	private CanvasGroup m_TargetCanvasGroup;

	[SerializeField]
	private OwlcatTransition m_Transition = OwlcatTransition.ColorTint;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private OwlcatSelectableActiveBlock m_ActiveBlock = OwlcatSelectableActiveBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableCanvasGroupBlock m_CanvasGroupBlock = OwlcatSelectableCanvasGroupBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableSpriteSwapBlock m_SpriteSwap;

	[SerializeField]
	private OwlcatSelectableColorLibraryBlock m_ColorLibrary = OwlcatSelectableColorLibraryBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableUnityAnimationBlock m_UnityAnimation = OwlcatSelectableUnityAnimationBlock.DefaultUnityAnimationBlock;

	[SerializeField]
	private OwlcatSelectableTweenAnimationBlock m_TweenAnimation;

	private bool m_IsActive = true;

	private OwlcatSelectionState m_LastState = OwlcatSelectionState.Normal;

	private CanvasFader m_CanvasFader;

	public bool IsActive
	{
		get
		{
			return m_IsActive;
		}
		set
		{
			m_IsActive = value;
			DoPartTransitionInternal(m_LastState, instant: true);
		}
	}

	public Image Image
	{
		get
		{
			return m_TargetGraphic as Image;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public Graphic TargetGraphic
	{
		get
		{
			return m_TargetGraphic;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			return m_TargetCanvasGroup;
		}
		set
		{
			m_TargetCanvasGroup = value;
		}
	}

	public OwlcatTransition Transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public ColorBlock Colors
	{
		get
		{
			return m_Colors;
		}
		set
		{
			m_Colors = value;
		}
	}

	public SpriteState SpriteState
	{
		get
		{
			return m_SpriteState;
		}
		set
		{
			m_SpriteState = value;
		}
	}

	public OwlcatSelectableActiveBlock ActiveBlock
	{
		get
		{
			return m_ActiveBlock;
		}
		set
		{
			m_ActiveBlock = value;
		}
	}

	public OwlcatSelectableCanvasGroupBlock CanvasGroupBlock
	{
		get
		{
			return m_CanvasGroupBlock;
		}
		set
		{
			m_CanvasGroupBlock = value;
		}
	}

	public OwlcatSelectableSpriteSwapBlock SpriteSwap
	{
		get
		{
			return m_SpriteSwap;
		}
		set
		{
			m_SpriteSwap = value;
		}
	}

	public OwlcatSelectableColorLibraryBlock ColorLibrary
	{
		get
		{
			return m_ColorLibrary;
		}
		set
		{
			m_ColorLibrary = value;
		}
	}

	public OwlcatSelectableUnityAnimationBlock UnityAnimation
	{
		get
		{
			return m_UnityAnimation;
		}
		set
		{
			m_UnityAnimation = value;
		}
	}

	public OwlcatSelectableTweenAnimationBlock TweenAnimationBlock
	{
		get
		{
			return m_TweenAnimation;
		}
		set
		{
			m_TweenAnimation = value;
		}
	}

	public virtual void DoPartTransition(OwlcatSelectionState state, bool instant)
	{
		m_LastState = state;
		if (IsActive)
		{
			DoPartTransitionInternal(state, instant);
		}
	}

	private void DoPartTransitionInternal(OwlcatSelectionState state, bool instant)
	{
		if (m_Transition != 0)
		{
			Color color;
			Color targetColor;
			Sprite newSprite;
			Sprite newSprite2;
			bool state2;
			float state3;
			Color targetColor2;
			string triggerName;
			TweenBehavior tweenAnimation;
			switch (state)
			{
			case OwlcatSelectionState.Normal:
				color = m_Colors.normalColor;
				targetColor = m_Colors.normalColor;
				newSprite = null;
				newSprite2 = m_SpriteSwap.normalSprite;
				state2 = m_ActiveBlock.Normal;
				state3 = m_CanvasGroupBlock.Normal;
				targetColor2 = m_ColorLibrary.Normal.Color;
				triggerName = m_UnityAnimation.NormalTrigger;
				tweenAnimation = m_TweenAnimation.NormalTween;
				break;
			case OwlcatSelectionState.Focused:
				color = m_Colors.selectedColor;
				targetColor = m_Colors.selectedColor;
				newSprite = m_SpriteState.selectedSprite;
				newSprite2 = m_SpriteSwap.focusedSprite;
				state2 = m_ActiveBlock.Selected;
				state3 = m_CanvasGroupBlock.Selected;
				targetColor2 = m_ColorLibrary.Focused.Color;
				triggerName = m_UnityAnimation.FocusedTrigger;
				tweenAnimation = m_TweenAnimation.FocusedTween;
				break;
			case OwlcatSelectionState.Highlighted:
				color = m_Colors.highlightedColor;
				targetColor = m_Colors.highlightedColor;
				newSprite = m_SpriteState.highlightedSprite;
				newSprite2 = m_SpriteSwap.highlightedSprite;
				state2 = m_ActiveBlock.Highlighted;
				state3 = m_CanvasGroupBlock.Highlighted;
				targetColor2 = m_ColorLibrary.Highlighted.Color;
				triggerName = m_UnityAnimation.HighlightedTrigger;
				tweenAnimation = m_TweenAnimation.HighlightedTween;
				break;
			case OwlcatSelectionState.Pressed:
				color = m_Colors.pressedColor;
				targetColor = m_Colors.pressedColor;
				newSprite = m_SpriteState.pressedSprite;
				newSprite2 = m_SpriteSwap.pressedSprite;
				state2 = m_ActiveBlock.Pressed;
				state3 = m_CanvasGroupBlock.Pressed;
				targetColor2 = m_ColorLibrary.Pressed.Color;
				triggerName = m_UnityAnimation.PressedTrigger;
				tweenAnimation = m_TweenAnimation.PressedTween;
				break;
			case OwlcatSelectionState.Disabled:
				color = m_Colors.disabledColor;
				targetColor = m_Colors.disabledColor;
				newSprite = m_SpriteState.disabledSprite;
				newSprite2 = m_SpriteSwap.disabledSprite;
				state2 = m_ActiveBlock.Disabled;
				state3 = m_CanvasGroupBlock.Disabled;
				targetColor2 = m_ColorLibrary.Disabled.Color;
				triggerName = m_UnityAnimation.DisabledTrigger;
				tweenAnimation = m_TweenAnimation.DisabledTween;
				break;
			default:
				color = Color.black;
				targetColor = m_Colors.pressedColor;
				newSprite = null;
				newSprite2 = null;
				state2 = false;
				state3 = 0f;
				targetColor2 = Color.black;
				triggerName = string.Empty;
				tweenAnimation = null;
				break;
			}
			switch (m_Transition)
			{
			case OwlcatTransition.ColorTint:
				StartColorTween(color * m_Colors.colorMultiplier, instant);
				break;
			case OwlcatTransition.SpriteSwapLegacy:
				DoSpriteSwap(newSprite);
				break;
			case OwlcatTransition.Activate:
				DoActiveState(state2);
				break;
			case OwlcatTransition.CanvasGroup:
				DoCanvasGroupState(state3, instant);
				break;
			case OwlcatTransition.SpriteSwap:
				DoSpriteSwap(newSprite2);
				break;
			case OwlcatTransition.ColorLibrary:
				StartColorTween(targetColor2, instant);
				break;
			case OwlcatTransition.ColorReplace:
				StartColorReplace(targetColor);
				break;
			case OwlcatTransition.UnityAnimation:
				DoUnityAnimation(triggerName);
				break;
			case OwlcatTransition.TweenAnimation:
				DoDoTweenAnimation(tweenAnimation);
				break;
			}
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null) && IsActive)
		{
			m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void StartColorReplace(Color targetColor)
	{
		if (!(m_TargetGraphic == null) && IsActive)
		{
			m_TargetGraphic.color = targetColor;
		}
	}

	private void DoCanvasGroupState(float state, bool instant)
	{
		if (CanvasGroup == null || !IsActive || CanvasGroup.alpha.Equals(state) || CanvasGroup.alpha == state)
		{
			return;
		}
		if (instant || Mathf.Approximately(m_CanvasGroupBlock.FadeDuration, 0f))
		{
			CanvasGroup.alpha = state;
			return;
		}
		if (m_CanvasFader == null)
		{
			m_CanvasFader = new CanvasFader(CanvasGroup, delegate
			{
				m_CanvasFader = null;
			});
		}
		m_CanvasFader.StartFade(state, m_CanvasGroupBlock.FadeDuration);
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(Image == null) && IsActive)
		{
			Image.overrideSprite = newSprite;
		}
	}

	private void DoDoTweenAnimation(TweenBehavior tweenAnimation)
	{
		if (IsActive)
		{
			tweenAnimation?.Play();
		}
	}

	private void DoUnityAnimation(string triggerName)
	{
		if (IsActive)
		{
			m_UnityAnimation.DoAnimation(triggerName);
		}
	}

	private void DoActiveState(bool state)
	{
		if (!(m_TargetGameObject == null))
		{
			state &= IsActive;
			if (state != m_TargetGameObject.activeSelf)
			{
				m_TargetGameObject.SetActive(state);
			}
		}
	}
}
