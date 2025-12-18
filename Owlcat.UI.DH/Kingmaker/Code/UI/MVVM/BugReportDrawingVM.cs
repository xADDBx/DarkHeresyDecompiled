using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BugReportDrawingVM : ViewModel
{
	private readonly Action m_CloseCallback;

	public BugReportDrawingVM(Action closeCallback)
	{
		m_CloseCallback = closeCallback;
	}

	public void Close()
	{
		m_CloseCallback();
	}
}
