using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UI.Common.Animations;

public class ScaleAnimator : MonoBehaviour, IUIAnimator
{
	[SerializeField]
	private Vector3 m_BaseScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 m_Scale = new Vector3(1.2f, 1.2f, 1.2f);

	[SerializeField]
	private float m_ScaleTime = 0.2f;

	[SerializeField]
	private int m_LoopsCount = -1;

	[SerializeField]
	private AnimationCurve m_AppearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve m_DisappearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Tweener m_AppearTweener;

	private Tweener m_CustomTweener;

	private Tweener m_DisappearTweener;

	private RectTransform m_RectTransform;

	private bool m_IsInit;

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		if (!m_IsInit && !(RectTransform == null))
		{
			RectTransform.localScale = m_BaseScale;
			m_IsInit = true;
		}
	}

	public void PlayOnce()
	{
		AppearAnimation(delegate
		{
			DisappearAnimation();
		});
	}

	public void AppearAnimation(UnityAction action = null)
	{
		Initialize();
		if (m_AppearTweener == null)
		{
			CreateAppearTweener();
		}
		m_DisappearTweener.Pause();
		m_CustomTweener.Pause();
		m_AppearTweener.OnComplete(delegate
		{
			action?.Invoke();
		});
		m_AppearTweener.Restart();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		Initialize();
		m_AppearTweener.Pause();
		m_CustomTweener.Pause();
		if (m_DisappearTweener == null)
		{
			CreateDisappearTweener();
		}
		m_DisappearTweener.Restart();
		action?.Invoke();
	}

	public void DoCustomAnimation(Vector3 scale, float scaleTime, AnimationCurve curve, bool loop)
	{
		Initialize();
		if (m_CustomTweener == null)
		{
			CreateCustomTweener();
		}
		m_DisappearTweener.Pause();
		m_AppearTweener.Pause();
		m_CustomTweener.Pause();
		m_CustomTweener.ChangeValues(m_RectTransform.localScale, scale, scaleTime);
		m_CustomTweener.SetEase(curve);
		m_CustomTweener.SetLoops(loop ? (-1) : 0);
		m_CustomTweener.Restart();
	}

	private void CreateCustomTweener()
	{
		m_CustomTweener = RectTransform.DOScale(1f, 1f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false)
			.Pause()
			.SetLoops(m_LoopsCount, LoopType.Yoyo);
	}

	private void CreateAppearTweener()
	{
		m_AppearTweener = RectTransform.DOScale(m_Scale, m_ScaleTime).SetEase(m_AppearCurve).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: false)
			.Pause()
			.SetLoops(m_LoopsCount, LoopType.Yoyo);
	}

	private void CreateDisappearTweener()
	{
		m_DisappearTweener = RectTransform.DOScale(m_BaseScale, m_ScaleTime).SetEase(m_DisappearCurve).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: false)
			.Pause();
	}

	public void DestroyViewImplementation()
	{
		m_AppearTweener.Kill();
		m_AppearTweener = null;
		m_DisappearTweener.Kill();
		m_DisappearTweener = null;
		m_IsInit = false;
	}

	public void OnDestroy()
	{
		DestroyViewImplementation();
	}

	public void SetValuesImmediately(Vector3 scale)
	{
		RectTransform.localScale = scale;
	}
}
