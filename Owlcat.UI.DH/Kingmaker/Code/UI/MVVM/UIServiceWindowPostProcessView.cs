using System;
using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class UIServiceWindowPostProcessView : MonoBehaviour
{
	[SerializeField]
	private RawImage m_ScreenContent;

	[SerializeField]
	private UIPostProcessingAnimator m_PostProcessingAnimator;

	[SerializeField]
	private FadeAnimator m_ScreenContentFadeAnimator;

	[ShowIf("m_HasShatteredGlass")]
	[SerializeField]
	private GameObject m_ShatteredGlass;

	[Header("Values")]
	[SerializeField]
	private bool m_HasShatteredGlass = true;

	public void Initialize()
	{
		m_ScreenContent.color = Color.clear;
		m_ScreenContentFadeAnimator.Initialize();
	}

	public void ShowFrom(UIPostEffectState prevState)
	{
		if (m_HasShatteredGlass)
		{
			m_ShatteredGlass.Or(null)?.SetActive(value: true);
		}
		m_ScreenContentFadeAnimator.AppearAnimation();
		m_PostProcessingAnimator.Bind(prevState);
		UIPostProcessSpace.Instance.Push(m_ScreenContent);
		m_ScreenContent.color = Color.white;
		m_PostProcessingAnimator.PlayState(UIPostEffectState.Default);
	}

	public void Hide(bool immediate, Action onHide = null)
	{
		if (m_HasShatteredGlass)
		{
			m_ShatteredGlass.Or(null)?.SetActive(value: false);
		}
		if (immediate)
		{
			DoClose();
			return;
		}
		m_PostProcessingAnimator.PlayState(UIPostEffectState.Off);
		m_ScreenContentFadeAnimator.DisappearAnimation(DoClose);
		void DoClose()
		{
			Dispose();
			onHide?.Invoke();
		}
	}

	private void Dispose()
	{
		m_ScreenContent.color = Color.clear;
		UIPostProcessSpace.Instance.Pop(m_ScreenContent);
		m_PostProcessingAnimator.Dispose();
	}
}
