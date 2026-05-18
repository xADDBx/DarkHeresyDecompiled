using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CursorTextWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject m_TextGroup;

	[SerializeField]
	private TMP_Text m_UpperText;

	[SerializeField]
	private TMP_Text m_LowerText;

	[SerializeField]
	private GameObject m_Separator;

	public void SetText(string upper, string lower)
	{
		bool flag = !string.IsNullOrEmpty(upper);
		bool flag2 = !string.IsNullOrEmpty(lower);
		bool flag3 = flag || flag2;
		m_TextGroup.SetActive(flag3);
		if (flag3)
		{
			m_Separator.SetActive(flag && flag2);
			m_UpperText.SetText(upper ?? string.Empty);
			m_LowerText.SetText(lower ?? string.Empty);
		}
	}
}
