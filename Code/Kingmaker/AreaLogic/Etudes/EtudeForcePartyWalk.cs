using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("92531dfac525406983bf7c2fdc6d5a9f")]
public class EtudeForcePartyWalk : EtudeBracketTrigger
{
	protected override void OnActivate()
	{
		base.OnActivate();
		Game.Instance.Player.ForcedWalk = true;
	}

	protected override void OnDeactivate()
	{
		Game.Instance.Player.ForcedWalk = false;
		base.OnDeactivate();
	}
}
