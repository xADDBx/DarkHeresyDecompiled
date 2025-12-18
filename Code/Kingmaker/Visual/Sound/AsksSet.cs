using System;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class AsksSet
{
	public AsksContainer Entries = new AsksContainer();

	public float Cooldown;

	public bool OverrideCooldownOnGamepad;

	[ShowIf("OverrideCooldownOnGamepad")]
	public float CooldownGamepad;

	public bool InterruptOthers;

	public float DelayMin;

	public float DelayMax;

	[Range(0f, 1f)]
	public float Chance = 1f;

	public bool ShowOnScreen;

	public bool DoNotPlayWhileAlone;

	public bool EnablePrioritization;

	[ShowIf("EnablePrioritization")]
	[Range(0f, 10f)]
	public int PrioritizationGroup;

	[ShowIf("EnablePrioritization")]
	[Range(0f, 10f)]
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
