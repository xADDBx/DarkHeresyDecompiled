using System;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class AsksSet
{
	public AsksContainer Entries = new AsksContainer();

	public float Cooldown;

	public bool OverrideCooldownOnGamepad;

	[ShowIf("OverrideCooldownOnGamepad")]
	public float CooldownGamepad;

	[Tooltip("Will interrupt currently playing ask")]
	[FormerlySerializedAs("InterruptOthers")]
	public bool InterruptCurrent;

	[Tooltip("Will erase queue before scheduling")]
	[ShowIf("InterruptCurrent")]
	public bool ClearQueue;

	[Tooltip("While playing, no other ask can be scheduled \n Example: Death ask")]
	public bool IsForbidQueueing;

	[Tooltip("Won't be played if other ask playing or queue not empty \n Example: Idle asks playing only if no other asks playing")]
	public bool CannotBePlayedIfQueueNotEmpty;

	public float DelayMin;

	public float DelayMax;

	[Range(0f, 1f)]
	public float Chance = 1f;

	public bool ShowOnScreen;

	public bool DoNotPlayWhileAlone;

	[Tooltip("Higher the number - higher the priority")]
	public int Priority;

	public float GetCooldown()
	{
		if (!OverrideCooldownOnGamepad || !Game.Instance.IsControllerGamepad)
		{
			return Cooldown;
		}
		return CooldownGamepad;
	}

	public virtual bool CheckBarkChance(float chanceValue)
	{
		return PFStatefulRandom.Visuals.Sounds.value <= chanceValue;
	}
}
