using Kingmaker.Code.Framework.GameLog;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CombatLogBaseVM : VirtualListElementVMBase
{
	public readonly CombatLogMessage Message;

	public CombatLogBaseVM(CombatLogMessage message)
	{
		Message = message;
	}

	protected override void DisposeImplementation()
	{
	}
}
