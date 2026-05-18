using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Framework.ElementSystem.Debug;

[TypeId("1ed10f7524074f609d7a995efa93d3ac")]
public class PausePlayModeAction : GameAction
{
	public override string GetCaption()
	{
		return "Pause Unity Play Mode";
	}

	protected override void RunAction()
	{
	}
}
