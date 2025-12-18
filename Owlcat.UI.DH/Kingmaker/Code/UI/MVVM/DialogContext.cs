using System;
using Code.View.UI.MVVM.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DialogContext : ViewModel
{
	private readonly ReactiveProperty<BookEventVM> m_BookEventVM;

	private readonly ReactiveProperty<EpilogVM> m_EpilogVM;

	private readonly ReactiveProperty<DialogVM> m_DialogVM;

	private readonly ReactiveProperty<DetectiveEpilogVM> m_DetectiveEpilogVM;

	public bool HasDialog => m_DialogVM.CurrentValue != null;

	public DialogContext(ReactiveProperty<BookEventVM> bookEventVm, ReactiveProperty<EpilogVM> epilogVm, ReactiveProperty<DialogVM> dialogVm, ReactiveProperty<DetectiveEpilogVM> detectiveEpilogVM)
	{
		m_BookEventVM = bookEventVm;
		m_EpilogVM = epilogVm;
		m_DialogVM = dialogVm;
		m_DetectiveEpilogVM = detectiveEpilogVM;
		GameUIState.Instance.ActiveDialogController.Subscribe(DialogControllerChanged).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeDialogs();
	}

	private void DialogControllerChanged(DialogController dialogController)
	{
		if (dialogController?.Dialog == null)
		{
			DisposeDialogs();
			return;
		}
		switch (dialogController.Dialog.Type)
		{
		case DialogType.Common:
			m_DialogVM.Value = new DialogVM().AddTo(this);
			break;
		case DialogType.Book:
			m_BookEventVM.Value = new BookEventVM().AddTo(this);
			break;
		case DialogType.Epilog:
			m_EpilogVM.Value = new EpilogVM().AddTo(this);
			break;
		case DialogType.Detective:
			m_DetectiveEpilogVM.Value = new DetectiveEpilogVM().AddTo(this);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void DisposeDialogs()
	{
		m_DialogVM.CurrentValue?.DetachView();
		m_DialogVM.CurrentValue?.Dispose();
		m_DialogVM.Value = null;
		m_BookEventVM.CurrentValue?.Dispose();
		m_BookEventVM.Value = null;
		m_EpilogVM.CurrentValue?.Dispose();
		m_EpilogVM.Value = null;
		m_DetectiveEpilogVM.CurrentValue?.Dispose();
		m_DetectiveEpilogVM.Value = null;
	}
}
