using System;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Actions;

[Serializable]
[TypeId("8451210a8ce24473bb60b412eedaf9e0")]
public class SetInteractionGlobalCooldown : GameAction
{
	[SerializeField]
	private bool CooldownAsInBlueprintArea;

	[HideIf("CooldownAsInBlueprintArea")]
	[SerializeField]
	private bool RandomCooldown;

	[ShowIf("ShouldShowSingleCooldown")]
	public float InteractionGlobalCooldown;

	[ShowIf("ShouldShowRandomCooldown")]
	public float InteractionGlobalCooldownMin;

	[ShowIf("ShouldShowRandomCooldown")]
	public float InteractionGlobalCooldownMax;

	private bool ShouldShowSingleCooldown
	{
		get
		{
			if (!CooldownAsInBlueprintArea)
			{
				return !RandomCooldown;
			}
			return false;
		}
	}

	private bool ShouldShowRandomCooldown
	{
		get
		{
			if (!CooldownAsInBlueprintArea)
			{
				return RandomCooldown;
			}
			return false;
		}
	}

	public override string GetCaption()
	{
		return "Set interaction global cooldown";
	}

	protected override void RunAction()
	{
		InteractionGlobalCooldownController controller = Game.Instance.GetController<InteractionGlobalCooldownController>();
		if (controller != null)
		{
			if (CooldownAsInBlueprintArea)
			{
				controller.UpdateGlobalCooldown();
				return;
			}
			float cooldownSeconds = (RandomCooldown ? PFStatefulRandom.Bark.Range(InteractionGlobalCooldownMin, InteractionGlobalCooldownMax) : InteractionGlobalCooldown);
			controller.UpdateGlobalCooldown(cooldownSeconds);
		}
	}
}
