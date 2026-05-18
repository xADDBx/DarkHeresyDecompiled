using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityTagWidget : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_Text;

	public void SetText(string text)
	{
		m_Text.SetText(text);
	}
}
