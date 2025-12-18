using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.Test;

public class ClassifiedText : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_TestText;

	[SerializeField]
	private RectTransform m_ClassifiedRt1;

	[SerializeField]
	private RectTransform m_ClassifiedRt2;

	[ContextMenu("SetRectangles")]
	private void SetRectangles()
	{
		string text = m_TestText.text.Replace("\u00ad", " ");
		int num = text.IndexOf('?');
		int num2 = text.LastIndexOf('?');
		TMP_CharacterInfo firstLetter = m_TestText.textInfo.characterInfo[num];
		TMP_CharacterInfo lastLetter = m_TestText.textInfo.characterInfo[num2];
		if (firstLetter.lineNumber == lastLetter.lineNumber)
		{
			CalculateClassifiedCoordinates(firstLetter, lastLetter, out var height, out var width, out var middlePoint);
			m_ClassifiedRt1.anchoredPosition = middlePoint;
			m_ClassifiedRt1.sizeDelta = new Vector2(width, height);
			m_ClassifiedRt1.gameObject.SetActive(value: true);
			m_ClassifiedRt2.gameObject.SetActive(value: false);
			return;
		}
		int num3 = 1;
		for (int i = num; i < num2; i++)
		{
			if (m_TestText.textInfo.characterInfo[i].lineNumber != firstLetter.lineNumber)
			{
				num3 = i;
				break;
			}
		}
		TMP_CharacterInfo lastLetter2 = m_TestText.textInfo.characterInfo[num3 - 1];
		TMP_CharacterInfo firstLetter2 = m_TestText.textInfo.characterInfo[num3];
		CalculateClassifiedCoordinates(firstLetter, lastLetter2, out var height2, out var width2, out var middlePoint2);
		CalculateClassifiedCoordinates(firstLetter2, lastLetter, out var height3, out var width3, out var middlePoint3);
		m_ClassifiedRt1.anchoredPosition = middlePoint2;
		m_ClassifiedRt1.sizeDelta = new Vector2(width2, height2);
		m_ClassifiedRt1.gameObject.SetActive(value: true);
		m_ClassifiedRt2.anchoredPosition = middlePoint3;
		m_ClassifiedRt2.sizeDelta = new Vector2(width3, height3);
		m_ClassifiedRt2.gameObject.SetActive(value: true);
	}

	private static void CalculateClassifiedCoordinates(TMP_CharacterInfo firstLetter, TMP_CharacterInfo lastLetter, out float height, out float width, out Vector2 middlePoint)
	{
		height = Mathf.Abs(firstLetter.topLeft.y - firstLetter.bottomRight.y);
		width = lastLetter.topRight.x - firstLetter.topLeft.x;
		middlePoint = new Vector2(firstLetter.topLeft.x + width / 2f, firstLetter.topRight.y - height / 2f);
	}
}
