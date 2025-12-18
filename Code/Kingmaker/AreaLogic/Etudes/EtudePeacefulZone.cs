using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("8b25726bac4999347b6946ed9ade440e")]
public class EtudePeacefulZone : EtudeBracketTrigger
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.Peaceful.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.Peaceful.Release();
	}

	protected override void OnResume()
	{
		OnEnter();
	}
}
