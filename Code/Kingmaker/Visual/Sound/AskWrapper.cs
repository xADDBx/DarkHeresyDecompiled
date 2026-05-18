using System;
using System.Linq;

namespace Kingmaker.Visual.Sound;

public class AskWrapper
{
	public readonly AsksSet AsksSet;

	public readonly string Type;

	public readonly UnitAsksManager UnitAsksManager;

	public float LastPlayTime = -100f;

	public bool IsPlaying;

	private readonly int[] m_ExclusionCounters;

	public bool IsOnCooldown
	{
		get
		{
			if (!IsPlaying)
			{
				return Game.Instance.Controllers.TimeController.RealTime.TotalSeconds < (double)(LastPlayTime + AsksSet.GetCooldown());
			}
			return true;
		}
	}

	public bool HasBarks
	{
		get
		{
			if (AsksSet.Chance > 0f)
			{
				return AsksSet.Entries.Any((AskEntry e) => e.IsExist);
			}
			return false;
		}
	}

	public int GetExclusionCounter(int entryIndex)
	{
		if ((uint)entryIndex >= (uint)m_ExclusionCounters.Length)
		{
			return 0;
		}
		return m_ExclusionCounters[entryIndex];
	}

	public void SetExclusionCounter(int entryIndex, int value)
	{
		if ((uint)entryIndex < (uint)m_ExclusionCounters.Length)
		{
			m_ExclusionCounters[entryIndex] = value;
		}
	}

	public void DecrementExclusionCounter(int entryIndex)
	{
		if ((uint)entryIndex < (uint)m_ExclusionCounters.Length && m_ExclusionCounters[entryIndex] > 0)
		{
			m_ExclusionCounters[entryIndex]--;
		}
	}

	public AskWrapper(AsksSet asksSet, UnitAsksManager asksManager, string askType = "")
	{
		AsksSet = asksSet;
		Type = askType;
		UnitAsksManager = asksManager;
		m_ExclusionCounters = new int[(asksSet?.Entries?.Length).GetValueOrDefault()];
	}

	[Obsolete]
	public AskWrapper(Bark bark, UnitAsksManager asksManager)
	{
		AsksSet = new AsksSet();
		CopyBarkToAskSet(AsksSet, bark);
		UnitAsksManager = asksManager;
		m_ExclusionCounters = new int[AsksSet.Entries?.Length ?? 0];
	}

	public static void CopyBarkToAskSet(AsksSet askSetBp, Bark askSet)
	{
		askSetBp.Cooldown = askSet.Cooldown;
		askSetBp.OverrideCooldownOnGamepad = askSet.OverrideCooldownOnGamepad;
		askSetBp.CooldownGamepad = askSet.CooldownGamepad;
		askSetBp.InterruptCurrent = askSet.InterruptOthers;
		askSetBp.DelayMin = askSet.DelayMin;
		askSetBp.DelayMax = askSet.DelayMax;
		askSetBp.Chance = askSet.Chance;
		askSetBp.ShowOnScreen = askSet.ShowOnScreen;
		askSetBp.DoNotPlayWhileAlone = askSet.DoNotPlayWhileAlone;
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
