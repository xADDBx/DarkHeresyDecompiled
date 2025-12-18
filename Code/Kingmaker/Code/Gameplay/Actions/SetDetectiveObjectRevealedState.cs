using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("129b7921225b490cafd1b776ea1b3d49")]
public class SetDetectiveObjectRevealedState : GameAction
{
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool IsRevealed;

	public override string GetCaption()
	{
		return $"Set Detective Object Revealed State ({IsRevealed} to {MapObject})";
	}

	protected override void RunAction()
	{
		MapObject.GetValue().DetectiveObject?.SetRevealed(IsRevealed);
	}
}
