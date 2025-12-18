using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CommonClueInformationDecorView : ClueInformationDecorBaseView
{
	[SerializeField]
	private Image m_SymbolIcon;

	[SerializeField]
	private TMP_Text m_SymbolsLabel;

	protected override void OnBind()
	{
		base.OnBind();
	}
}
