using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Actions;

[Serializable]
[TypeId("91541831892c4967827dbdd7d9b92705")]
public class ResetAwarenessCheck : GameAction
{
	public MapObjectEvaluator MapObject;

	public override string GetCaption()
	{
		return $"Reset awareness check for {MapObject}";
	}

	protected override void RunAction()
	{
		MapObject.GetValue().GetOptional<PartAwarenessCheck>()?.Reset();
	}
}
