using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseScreenVM : ViewModel
{
	public readonly bool CanCloseCase;

	private readonly ReactiveProperty<OpenedCaseAnnotationsVM> m_AnnotationsVM = new ReactiveProperty<OpenedCaseAnnotationsVM>();

	private readonly ReactiveProperty<OpenedCaseTransformControlsVM> m_OpenedCaseTransformControlsVM = new ReactiveProperty<OpenedCaseTransformControlsVM>();

	private readonly Action m_OnCloseAction;

	public ReadOnlyReactiveProperty<OpenedCaseAnnotationsVM> AnnotationsVM => m_AnnotationsVM;

	public ReadOnlyReactiveProperty<OpenedCaseTransformControlsVM> OpenedCaseTransformControlsVM => m_OpenedCaseTransformControlsVM;

	public OpenedCaseScreenVM(Action onClose)
	{
		CanCloseCase = onClose != null;
		m_OnCloseAction = onClose;
		m_OpenedCaseTransformControlsVM.Value = new OpenedCaseTransformControlsVM();
	}

	private void SetAnnotationsState(bool state)
	{
		if (state)
		{
			m_AnnotationsVM.Value?.Dispose();
			m_AnnotationsVM.Value = new OpenedCaseAnnotationsVM();
		}
		else
		{
			m_AnnotationsVM.Value.Dispose();
			m_AnnotationsVM.Value = null;
		}
	}

	public void ToggleAnnotations()
	{
		if (m_AnnotationsVM.Value != null)
		{
			m_AnnotationsVM.Value.Dispose();
			m_AnnotationsVM.Value = null;
		}
		else
		{
			m_AnnotationsVM.Value = new OpenedCaseAnnotationsVM();
		}
	}

	public void Close()
	{
		m_OnCloseAction?.Invoke();
	}
}
