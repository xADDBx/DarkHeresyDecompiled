using System;
using System.Linq;

namespace Kingmaker.Visual.Sound;

public class AskWrapper
{
	public readonly AsksSet Bark;

	public readonly UnitAsksManager UnitAsksManager;

	public float LastPlayTime = -100f;

	public bool IsPlaying;

	public bool IsOnCooldown
	{
		get
		{
			if (!IsPlaying)
			{
				return Game.Instance.Controllers.TimeController.RealTime.TotalSeconds < (double)(LastPlayTime + Bark.GetCooldown());
			}
			return true;
		}
	}

	public bool HasBarks
	{
		get
		{
			if (Bark.Chance > 0f)
			{
				return Bark.Entries.Any((AskEntry e) => !e.IsEmpty);
			}
			return false;
		}
	}

	public AskWrapper(AsksSet bark, UnitAsksManager asksManager)
	{
		Bark = bark;
		UnitAsksManager = asksManager;
	}

	[Obsolete]
	public AskWrapper(Bark bark, UnitAsksManager asksManager)
	{
		Bark = new AsksSet();
		CopyBarkToAskSet(Bark, bark);
		UnitAsksManager = asksManager;
	}

	public static void CopyBarkToAskSet(AsksSet askSetBp, Bark askSet)
	{
		askSetBp.Cooldown = askSet.Cooldown;
		askSetBp.OverrideCooldownOnGamepad = askSet.OverrideCooldownOnGamepad;
		askSetBp.CooldownGamepad = askSet.CooldownGamepad;
		askSetBp.InterruptOthers = askSet.InterruptOthers;
		askSetBp.DelayMin = askSet.DelayMin;
		askSetBp.DelayMax = askSet.DelayMax;
		askSetBp.Chance = askSet.Chance;
		askSetBp.ShowOnScreen = askSet.ShowOnScreen;
		askSetBp.DoNotPlayWhileAlone = askSet.DoNotPlayWhileAlone;
		askSetBp.EnablePrioritization = askSet.EnablePrioritization;
		askSetBp.PrioritizationGroup = askSet.PrioritizationGroup;
		askSetBp.Priority = askSet.Priority;
		if (askSet.Entries != null)
		{
			for (int i = 0; i < askSet.Entries.Length; i++)
			{
				AskEntry value = askSet.Entries[i];
				askSetBp.Entries[i] = value;
			}
		}
	}
}
