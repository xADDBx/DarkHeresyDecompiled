using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickValueStatFormulaConsoleView : BrickValueStatFormulaView
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public OwlcatMultiButton MultiButton => m_MultiButton;
}
