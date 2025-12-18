using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Owlcat.UI.Commands;

public interface ICommandProvider
{
	IReadOnlyCollection<Command> Commands
	{
		[return: NotNull]
		get;
	}
}
