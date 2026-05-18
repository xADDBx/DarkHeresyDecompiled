using System.Collections.Generic;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

public static class EncounterCROverrideHelper
{
	private static readonly Dictionary<string, List<OverrideEncounterCR>> _Overrides = new Dictionary<string, List<OverrideEncounterCR>>();

	public static int? Get(string encounterAssetGuid)
	{
		if (encounterAssetGuid == null)
		{
			return null;
		}
		if (_Overrides.TryGetValue(encounterAssetGuid, out var value) && value.Count > 0)
		{
			List<OverrideEncounterCR> list = value;
			return list[list.Count - 1].NewCR;
		}
		return null;
	}

	public static void Push(string encounterAssetGuid, OverrideEncounterCR component)
	{
		if (encounterAssetGuid != null && component != null)
		{
			if (!_Overrides.TryGetValue(encounterAssetGuid, out var value))
			{
				value = new List<OverrideEncounterCR>();
				_Overrides[encounterAssetGuid] = value;
			}
			value.Add(component);
		}
	}

	public static void Pop(string encounterAssetGuid, OverrideEncounterCR component)
	{
		if (encounterAssetGuid != null && component != null && _Overrides.TryGetValue(encounterAssetGuid, out var value))
		{
			value.Remove(component);
			if (value.Count == 0)
			{
				_Overrides.Remove(encounterAssetGuid);
			}
		}
	}
}
