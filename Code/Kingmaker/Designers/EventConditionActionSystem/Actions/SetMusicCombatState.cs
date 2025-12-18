using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("9477b45b5ec2479f98e7d60efdb51f53")]
public class SetMusicCombatState : GameAction
{
	public bool ClearForcedState;

	[HideIf("ClearForcedState")]
	[Tooltip("It overrides only state from PowerBalance")]
	[SerializeField]
	private UnitVisualSettings.MusicCombatState ForcedState = UnitVisualSettings.MusicCombatState.Regular;

	public override string GetCaption()
	{
		if (!ClearForcedState)
		{
			return $"Force music state in combat to {ForcedState}";
		}
		return "Clears forced state";
	}

	protected override void RunAction()
	{
		if (ClearForcedState)
		{
			Game.Instance.Controllers.MoraleController.ForcedMusicCombatState = UnitVisualSettings.MusicCombatState.None;
		}
		else
		{
			Game.Instance.Controllers.MoraleController.ForcedMusicCombatState = ForcedState;
		}
		SoundState.Instance?.MusicStateHandler.UpdateCombatMusicState();
	}
}
