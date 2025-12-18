using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ConsoleHintWidgetContainer : MonoBehaviour
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	public ConsoleHintsWidget GetConsoleHintWidget()
	{
		return m_ConsoleHintsWidget;
	}
}
