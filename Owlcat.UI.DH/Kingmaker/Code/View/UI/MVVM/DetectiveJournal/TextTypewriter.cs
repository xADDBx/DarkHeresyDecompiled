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

	private Coroutine m_TypewriterCoroutine;

	private WaitForSecondsRealtime m_SimpleDelayWfS;

	[Header("Typewriter Settings")]
	[SerializeField]
	private float m_CharactersPerSecond = 20f;

	public TMP_Text Text { get; private set; }

	private void Awake()
	{
		Text = GetComponent<TMP_Text>();
		m_SimpleDelayWfS = new WaitForSecondsRealtime(1f / m_CharactersPerSecond);
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
		m_TypewriterCoroutine = StartCoroutine(Typewriter());
	}

	private IEnumerator Typewriter()
	{
		TMP_TextInfo textInfo = Text.textInfo;
		while (m_CurrentVisibleCharacterIndex < textInfo.characterCount + 1)
		{
			if (m_CurrentVisibleCharacterIndex >= textInfo.characterCount - 1)
			{
				Text.maxVisibleCharacters++;
				break;
			}
			Text.maxVisibleCharacters++;
			m_CurrentVisibleCharacterIndex++;
			UISounds.Instance.PlayButtonClickSound();
			yield return m_SimpleDelayWfS;
		}
	}
}
