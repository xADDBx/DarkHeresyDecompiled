using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugReportPCView : BugReportBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
		m_CloseButton.OnConfirmClickAsObservable().Subscribe(base.OnClose).AddTo(this);
		m_DrawingButton.OnConfirmClickAsObservable().Subscribe(base.OnShowDrawing).AddTo(this);
		m_DuplicatesButton.OnConfirmClickAsObservable().Subscribe(base.OnShowDuplicates).AddTo(this);
		m_SendButton.OnConfirmClickAsObservable().Subscribe(base.OnSend).AddTo(this);
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.OnClose).AddTo(this);
		m_DrawingButton.OnLeftClickAsObservable().Subscribe(base.OnShowDrawing).AddTo(this);
		m_SendButton.OnLeftClickAsObservable().Subscribe(base.OnSend).AddTo(this);
		m_DuplicatesButton.OnLeftClickAsObservable().Subscribe(base.OnShowDuplicates).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.OnClose).AddTo(this);
		m_LabelsButton.OnConfirmClickAsObservable().Subscribe(base.OnLabelsShow).AddTo(this);
		m_LabelsButton.OnLeftClickAsObservable().Subscribe(base.OnLabelsShow).AddTo(this);
	}
}
