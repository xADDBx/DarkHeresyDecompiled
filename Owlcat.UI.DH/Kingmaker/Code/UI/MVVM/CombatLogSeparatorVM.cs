using Kingmaker.Code.Framework.GameLog;

namespace Kingmaker.Code.UI.MVVM;

public class CombatLogSeparatorVM : CombatLogBaseVM
{
	public CombatLogSeparatorVM(CombatLogMessage message)
		: base(message)
	{
	}

	protected override void DisposeImplementation()
	{
	}
}
