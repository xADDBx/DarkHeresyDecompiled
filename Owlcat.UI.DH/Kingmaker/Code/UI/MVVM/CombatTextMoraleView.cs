using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CombatTextMoraleView : CombatTextEntityBaseView<CombatMessageMorale>
{
	[SerializeField]
	private Image m_IconArrowUp;

	[SerializeField]
	private Image m_IconArrowDown;

	[SerializeField]
	private TextMeshProUGUI m_Text;

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
		SetArrows(flag);
		SetupSize();
	}

	private void SetupSize()
	{
		m_Text.ForceMeshUpdate();
		TextRectTransform.sizeDelta = new Vector2(m_Text.textBounds.size.x, TextRectTransform.sizeDelta.y);
		RectTransform.sizeDelta = new Vector2(m_Text.textBounds.size.x + (m_IconArrowUp.transform as RectTransform).sizeDelta.x, RectTransform.sizeDelta.y);
	}

	protected override void DoShow()
	{
		base.CanvasGroup.alpha = 0f;
		base.CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetArrows(bool isPositive)
	{
		m_IconArrowUp.color = new Color(1f, 1f, 1f, isPositive ? 1f : 0f);
		m_IconArrowDown.color = new Color(1f, 1f, 1f, isPositive ? 0f : 1f);
	}
}
