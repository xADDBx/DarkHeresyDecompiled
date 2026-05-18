using System;
using Owlcat.UI.Tweenable;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.UI;

[Serializable]
public class OwlcatSelectableLayerPart
{
	public class CanvasFader : IDisposable
	{
		private readonly CanvasGroup m_CanvasGroup;

		private float m_StartAlpha;

		private float m_TargetAlpha;

		private float m_FadeDuration;

		private float m_ElapsedTime;

		private bool m_IsFading;

		private IDisposable m_FadeAnimation;

		private readonly Action m_DisposeCallback;

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
			m_FadeAnimation?.Dispose();
			m_FadeAnimation = null;
			m_IsFading = false;
		}

		public void Dispose()
		{
			StopFading();
			m_DisposeCallback?.Invoke();
		}
	}

	private readonly struct StateData
	{
		public readonly Color Tint;

		public readonly Color Replace;

		public readonly Sprite LegacySprite;

		public readonly Sprite Sprite;

		public readonly bool Active;

		public readonly float CanvasAlpha;

		public readonly Color LibraryColor;

		public readonly string AnimTrigger;

		public readonly TweenBehavior Tween;

		public StateData(Color tint, Color replace, Sprite legacySprite, Sprite sprite, bool active, float canvasAlpha, Color libraryColor, string animTrigger, TweenBehavior tween)
		{
			Tint = tint;
			Replace = replace;
			LegacySprite = legacySprite;
			Sprite = sprite;
			Active = active;
			CanvasAlpha = canvasAlpha;
			LibraryColor = libraryColor;
			AnimTrigger = animTrigger;
			Tween = tween;
		}
	}

	[SerializeField]
	private OwlcatSelectableActiveBlock m_ActiveBlock = OwlcatSelectableActiveBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableUnityAnimationBlock m_UnityAnimation = OwlcatSelectableUnityAnimationBlock.DefaultUnityAnimationBlock;

	[SerializeField]
	private OwlcatSelectableTweenAnimationBlock m_TweenAnimation;

	[SerializeField]
	private CanvasGroup m_TargetCanvasGroup;

	[SerializeField]
	private OwlcatSelectableCanvasGroupBlock m_CanvasGroupBlock = OwlcatSelectableCanvasGroupBlock.DefaultActiveBlock;

	private CanvasFader m_CanvasFader;

	[SerializeField]
	private OwlcatSelectableColorLibraryBlock m_ColorLibrary = OwlcatSelectableColorLibraryBlock.DefaultActiveBlock;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private OwlcatSelectableSpriteSwapBlock m_SpriteSwap;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private Graphic m_TargetGraphic;

	[SerializeField]
	private GameObject m_TargetGameObject;

	[SerializeField]
	private OwlcatTransition m_Transition = OwlcatTransition.ColorTint;

	private bool m_IsActive = true;

	private OwlcatSelectionState m_LastState = OwlcatSelectionState.Normal;

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

	public bool IsActive
	{
		get
		{
			return m_IsActive;
		}
		set
		{
			if (m_IsActive != value)
			{
				m_IsActive = value;
				DoPartTransitionInternal(m_LastState, instant: true);
			}
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

	private void DoCanvasGroupState(float state, bool instant)
	{
		if (!IsActive || CanvasGroup == null || CanvasGroup.alpha.Equals(state))
		{
			return;
		}
		if (instant || Mathf.Approximately(m_CanvasGroupBlock.FadeDuration, 0f))
		{
			m_CanvasFader?.Dispose();
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

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (IsActive && !(m_TargetGraphic == null))
		{
			m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void StartColorReplace(Color targetColor)
	{
		if (IsActive && !(m_TargetGraphic == null))
		{
			m_TargetGraphic.color = targetColor;
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (IsActive && !(Image == null))
		{
			Image.overrideSprite = newSprite;
		}
	}

	private StateData GetStateData(OwlcatSelectionState s)
	{
		return s switch
		{
			OwlcatSelectionState.Normal => new StateData(m_Colors.normalColor, m_Colors.normalColor, null, m_SpriteSwap.normalSprite, m_ActiveBlock.Normal, m_CanvasGroupBlock.Normal, m_ColorLibrary.Normal.Color, m_UnityAnimation.NormalTrigger, m_TweenAnimation.NormalTween), 
			OwlcatSelectionState.Focused => new StateData(m_Colors.selectedColor, m_Colors.selectedColor, m_SpriteState.selectedSprite, m_SpriteSwap.focusedSprite, m_ActiveBlock.Selected, m_CanvasGroupBlock.Selected, m_ColorLibrary.Focused.Color, m_UnityAnimation.FocusedTrigger, m_TweenAnimation.FocusedTween), 
			OwlcatSelectionState.Highlighted => new StateData(m_Colors.highlightedColor, m_Colors.highlightedColor, m_SpriteState.highlightedSprite, m_SpriteSwap.highlightedSprite, m_ActiveBlock.Highlighted, m_CanvasGroupBlock.Highlighted, m_ColorLibrary.Highlighted.Color, m_UnityAnimation.HighlightedTrigger, m_TweenAnimation.HighlightedTween), 
			OwlcatSelectionState.Pressed => new StateData(m_Colors.pressedColor, m_Colors.pressedColor, m_SpriteState.pressedSprite, m_SpriteSwap.pressedSprite, m_ActiveBlock.Pressed, m_CanvasGroupBlock.Pressed, m_ColorLibrary.Pressed.Color, m_UnityAnimation.PressedTrigger, m_TweenAnimation.PressedTween), 
			OwlcatSelectionState.Disabled => new StateData(m_Colors.disabledColor, m_Colors.disabledColor, m_SpriteState.disabledSprite, m_SpriteSwap.disabledSprite, m_ActiveBlock.Disabled, m_CanvasGroupBlock.Disabled, m_ColorLibrary.Disabled.Color, m_UnityAnimation.DisabledTrigger, m_TweenAnimation.DisabledTween), 
			_ => new StateData(Color.black, m_Colors.pressedColor, null, null, active: false, 0f, Color.black, string.Empty, null), 
		};
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
			StateData stateData = GetStateData(state);
			switch (m_Transition)
			{
			case OwlcatTransition.ColorTint:
				StartColorTween(stateData.Tint * m_Colors.colorMultiplier, instant);
				break;
			case OwlcatTransition.SpriteSwapLegacy:
				DoSpriteSwap(stateData.LegacySprite);
				break;
			case OwlcatTransition.Activate:
				DoActiveState(stateData.Active);
				break;
			case OwlcatTransition.CanvasGroup:
				DoCanvasGroupState(stateData.CanvasAlpha, instant);
				break;
			case OwlcatTransition.SpriteSwap:
				DoSpriteSwap(stateData.Sprite);
				break;
			case OwlcatTransition.ColorLibrary:
				StartColorTween(stateData.LibraryColor, instant);
				break;
			case OwlcatTransition.ColorReplace:
				StartColorReplace(stateData.Replace);
				break;
			case OwlcatTransition.UnityAnimation:
				DoUnityAnimation(stateData.AnimTrigger);
				break;
			case OwlcatTransition.TweenAnimation:
				DoDoTweenAnimation(stateData.Tween);
				break;
			}
		}
	}
}
