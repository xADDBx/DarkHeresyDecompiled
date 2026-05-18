using System;
using Owlcat.UI.Commands;
using R3;

namespace Kingmaker.UI.Common;

public static class HintViewExtensions
{
	public static IDisposable SubscribeToView(this CommandHandle handle, HintView view)
	{
		return handle.Command.SubscribeToView(view);
	}

	public static IDisposable SubscribeToView(this Command command, HintView view)
	{
		view.Bind(command);
		return Disposable.Create(view.Unbind);
	}
}
