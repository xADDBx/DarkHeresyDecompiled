using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugReportDuplicatesPCView : BugReportDuplicatesBaseView
{
	[Header("PC Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	protected override void CreateInput()
	{
		base.CreateInput();
		m_BackButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
	}
}
