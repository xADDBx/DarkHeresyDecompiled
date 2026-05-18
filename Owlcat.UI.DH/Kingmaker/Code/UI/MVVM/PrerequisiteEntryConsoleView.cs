using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PrerequisiteEntryConsoleView : PrerequisiteEntryView
{
	[Header("Focus")]
	[SerializeField]
	private SingleLinkMultiButton m_Focus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;
}
