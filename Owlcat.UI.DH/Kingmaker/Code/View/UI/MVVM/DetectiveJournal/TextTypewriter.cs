using System.Collections;
using Kingmaker.UI.Sound;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[RequireComponent(typeof(TMP_Text))]
public class TextTypewriter : MonoBehaviour
{
	private int m_CurrentVisibleCharacterIndex;

	private int m_FirstLetter;

	private Coroutine m_TypewriterCoroutine;

	[Header("Typewriter Settings")]
	[SerializeField]
	[Tooltip("Recommended typing speed. Effective speed may be increased if the full animation would otherwise exceed Max Animation Time.")]
	private float m_CharactersPerSecond = 60f;

	[SerializeField]
	[Tooltip("Hard cap on total animation duration in seconds. If the text is too long for the recommended speed, base interval is scaled down so the whole animation fits.")]
	private float m_MaxAnimationTime = 2.5f;

	[SerializeField]
	[Tooltip("Speed multiplier over typing progress (0 = first letter, 1 = last). Values < 1 slow down, > 1 speed up.")]
	private AnimationCurve m_SpeedMultiplierCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f);

	[field: SerializeField]
	public TMP_Text Text { get; private set; }

	private void Awake()
	{
		Text = GetComponent<TMP_Text>();
	}

	private void OnDisable()
	{
		if (m_TypewriterCoroutine != null)
		{
			StopCoroutine(m_TypewriterCoroutine);
		}
	}

	public void SetTextForce(string text)
	{
		Text.text = text;
		Text.maxVisibleCharacters = text.Length - 1;
	}

	public void SetTextAnimation(string text, int firstTypewriterLetter = 0, bool force = false)
	{
		Text.text = text;
		Text.maxVisibleCharacters = (force ? (text.Length - 1) : firstTypewriterLetter);
		if (!force)
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				StartAnimation(firstTypewriterLetter);
			}).AddTo(this);
		}
	}

	private void StartAnimation(int firstLetter)
	{
		if (m_TypewriterCoroutine != null)
		{
			StopCoroutine(m_TypewriterCoroutine);
		}
		Text.maxVisibleCharacters = firstLetter;
		m_CurrentVisibleCharacterIndex = firstLetter;
		m_FirstLetter = firstLetter;
		m_TypewriterCoroutine = StartCoroutine(Typewriter());
	}

	private IEnumerator Typewriter()
	{
		TMP_TextInfo textInfo = Text.textInfo;
		float totalRange = Mathf.Max(1, textInfo.characterCount - m_FirstLetter);
		float num = 0f;
		for (int i = m_FirstLetter; i < textInfo.characterCount; i++)
		{
			float time = (float)(i - m_FirstLetter) / totalRange;
			float num2 = Mathf.Max(0.01f, m_SpeedMultiplierCurve.Evaluate(time));
			num += 1f / num2;
		}
		float num3 = 1f / m_CharactersPerSecond;
		float b = ((num > 0f) ? (m_MaxAnimationTime / num) : num3);
		float baseInterval = Mathf.Min(num3, b);
		float accumulated = 0f;
		while (m_CurrentVisibleCharacterIndex < textInfo.characterCount)
		{
			accumulated += Time.unscaledDeltaTime;
			bool flag = false;
			while (m_CurrentVisibleCharacterIndex < textInfo.characterCount)
			{
				float time2 = (float)(m_CurrentVisibleCharacterIndex - m_FirstLetter) / totalRange;
				float num4 = Mathf.Max(0.01f, m_SpeedMultiplierCurve.Evaluate(time2));
				float num5 = baseInterval / num4;
				if (accumulated < num5)
				{
					break;
				}
				Text.maxVisibleCharacters++;
				m_CurrentVisibleCharacterIndex++;
				accumulated -= num5;
				flag = true;
			}
			if (flag)
			{
				UISounds.Instance.PlayButtonClickSound();
			}
			yield return null;
		}
	}
}
