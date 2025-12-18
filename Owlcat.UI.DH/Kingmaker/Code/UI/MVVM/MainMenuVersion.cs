using Kingmaker.GameInfo;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuVersion : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private void Awake()
	{
		m_Label.text = GameVersion.GetVersion();
	}
}
