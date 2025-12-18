using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerRoundWidget : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_RootTransform;

	[SerializeField]
	private TMP_Text m_RoundText;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public Vector2 GetSize()
	{
		return m_RootTransform.sizeDelta;
	}

	public void SetRoundText(int round)
	{
		m_RoundText.SetText(round.ToString());
	}
}
