using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Actions;

[Serializable]
[TypeId("45c999732019b5f4d901ec5dd8f07400")]
public class ResetInteractionGlobalCooldown : GameAction
{
	public override string GetCaption()
	{
		return "Reset interaction global cooldown";
	}

	protected override void RunAction()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null)
		{
			loadedAreaState.InteractionGlobalCooldownExpiry = TimeSpan.Zero;
		}
	}
}
