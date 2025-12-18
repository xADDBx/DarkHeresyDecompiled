using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public interface IConsoleTooltipBrick
{
	bool IsBinded { get; }

	IConsoleEntity GetConsoleEntity();
}
