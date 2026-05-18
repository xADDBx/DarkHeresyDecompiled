using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatTextMoraleView : CombatTextEntityBaseView<CombatMessageMorale>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TMP_Text m_SignText;

	[SerializeField]
	private Color m_TextPositiveColor;

	[SerializeField]
	private Color m_TextNegativeColor;

	private RectTransform RectTransform => base.transform as RectTransform;

	private RectTransform TextRectTransform => m_Text.transform as RectTransform;

	protected override float GetXPos()
	{
		return 0f;
	}

	protected override void DoData(CombatMessageMorale combatMessage)
	{
		bool flag = combatMessage.Amount > 0;
		m_Text.text = combatMessage.GetText();
		m_Text.color = (flag ? m_TextPositiveColor : m_TextNegativeColor);
		SetSign(flag);
		SetupSize();
	}

	private void SetupSize()
	{
		m_Text.ForceMeshUpdate();
		m_SignText.ForceMeshUpdate();
		Vector3 size = m_Text.textBounds.size;
		TextRectTransform.sizeDelta = new Vector2(size.x, m_Text.textBounds.size.y);
		RectTransform.sizeDelta = new Vector2(size.x + m_SignText.rectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
	}

	protected override void DoShow()
	{
		base.CanvasGroup.alpha = 0f;
		base.CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetSign(bool isPositive)
	{
		m_SignText.color = (isPositive ? m_TextPositiveColor : m_TextNegativeColor);
		m_SignText.text = (isPositive ? "+" : "-");
	}
}
