using System;
using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class TextEntityWidget : IDisposable
{
	[Header("Values")]
	[SerializeField]
	private bool m_HideEmpty = true;

	[SerializeField]
	private bool m_IsGlossary = true;

	private AccessibilityTextHelper m_TextHelper;

	private CompositeDisposable m_Disposables = new CompositeDisposable();

	[field: SerializeField]
	public TMP_Text Text { get; private set; }

	public void Initialize()
	{
		Text.SetText(string.Empty);
		Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, m_IsGlossary));
		if (m_HideEmpty)
		{
			Text.gameObject.SetActive(value: false);
		}
	}

	public TextEntityWidget Bind(TextEntity textEntity)
	{
		if (textEntity == null)
		{
			Dispose();
			return null;
		}
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(Text).AddTo(m_Disposables);
		}
		AccessibilityTextHelper.ApplyParamsTo(Text, textEntity.TextParams);
		Text.SetText(textEntity.Text);
		Text.ApplyTextFieldParams(textEntity.TextParams);
		Text.gameObject.SetActive(!m_HideEmpty || !string.IsNullOrEmpty(textEntity.Text));
		m_TextHelper.UpdateTextSize();
		return this;
	}

	public void Dispose()
	{
		m_Disposables?.Dispose();
		Text.SetText(string.Empty);
		if (m_HideEmpty)
		{
			Text.gameObject.SetActive(value: false);
		}
	}
}
