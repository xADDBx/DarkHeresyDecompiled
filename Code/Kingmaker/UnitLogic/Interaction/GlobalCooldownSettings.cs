using System;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[Serializable]
public class GlobalCooldownSettings
{
	public bool UseGlobalCooldown;

	[Tooltip("Close interactions will unite to cluster for optimized firing. If this interaction moving (patrol), set this to false")]
	[ShowIf("UseGlobalCooldown")]
	public bool CanCluster = true;

	[Tooltip("TRUE: Cooldown will be set to value in BlueprintArea \nFALSE: cooldown will be set to SetCooldownTime value")]
	[ShowIf("UseGlobalCooldown")]
	public bool UseDefaultCooldownTime = true;

	[ShowIf("ShouldShowCustomTime")]
	public float SetCooldownTime;

	private bool ShouldShowCustomTime
	{
		get
		{
			if (UseGlobalCooldown)
			{
				return !UseDefaultCooldownTime;
			}
			return false;
		}
	}

	public void UpdateGlobalCooldown(IEntity source)
	{
		InteractionGlobalCooldownController controller = Game.Instance.GetController<InteractionGlobalCooldownController>();
		if (controller != null)
		{
			if (UseDefaultCooldownTime)
			{
				controller.UpdateGlobalCooldown(source);
			}
			else
			{
				controller.UpdateGlobalCooldown(SetCooldownTime, source);
			}
		}
	}
}
