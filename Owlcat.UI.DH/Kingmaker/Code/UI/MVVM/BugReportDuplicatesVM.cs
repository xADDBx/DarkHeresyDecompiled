using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BugReportDuplicatesVM : ViewModel
{
	public readonly string Context;

	private readonly Action m_CloseCallback;

	public BugReportDuplicatesVM(Action closeCallback, string context)
	{
		m_CloseCallback = closeCallback;
		Context = context;
	}

	public void Close()
	{
		m_CloseCallback();
	}
}
