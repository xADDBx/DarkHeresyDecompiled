using System;

namespace Owlcat.UI.Commands;

public static class CommandExtensions
{
	public static CommandHandle AddCommand(this IBindable view, string binding)
	{
		return view.AddCommand(binding, null, CommandPhase.Tunnel);
	}

	public static CommandHandle AddCommand(this IBindable view, string binding, Action action)
	{
		return view.AddCommand(binding, action, CommandPhase.Tunnel);
	}

	public static CommandHandle AddCommand(this IBindable view, string binding, Action action, CommandPhase phase)
	{
		return view.AddCommand(new Command(binding, action)
		{
			Phase = phase
		});
	}

	public static CommandHandle AddCommand(this IBindable view, Command command)
	{
		return new CommandHandle(command, view);
	}

	public static IDisposable SubscribeToHint(this CommandHandle command, CommandHint hint)
	{
		return command.SubscribeToHint(hint);
	}

	public static IDisposable SubscribeToHint(this Command command, CommandHint hint)
	{
		hint.Bind(command);
		return new DisposableAction(hint.Unbind);
	}
}
