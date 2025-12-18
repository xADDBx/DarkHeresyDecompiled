using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/BlueprintAlignmentMarksRoot")]
[TypeId("6d28fb4beac041dca984f5e7d9dbd205")]
public class BlueprintAlignmentMarksRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAlignmentMarksRoot>
	{
	}

	[SerializeField]
	private List<FactsOnAlignmentMark> AlignmentMarkToFacts = new List<FactsOnAlignmentMark>();

	[SerializeField]
	private List<AlignmentAxisStaticInfo> AlignmentStaticInfo = new List<AlignmentAxisStaticInfo>();

	private Dictionary<AlignmentAxis, List<int>> m_AxisToMarkRanksMap = new Dictionary<AlignmentAxis, List<int>>();

	private Dictionary<AlignmentAxis, AlignmentAxisStaticInfo> m_AlignmentStaticInfoMap = new Dictionary<AlignmentAxis, AlignmentAxisStaticInfo>();

	private Dictionary<AlignmentAxis, Dictionary<int, List<BlueprintMechanicEntityFact>>> m_AxisToMarkFactsMap = new Dictionary<AlignmentAxis, Dictionary<int, List<BlueprintMechanicEntityFact>>>();

	public List<BlueprintMechanicEntityFact> GetFactsOnMark(AlignmentAxis axis, int mark)
	{
		if (!m_AxisToMarkFactsMap.TryGetValue(axis, out var value))
		{
			return new List<BlueprintMechanicEntityFact>();
		}
		if (!value.TryGetValue(mark - 1, out var value2))
		{
			return new List<BlueprintMechanicEntityFact>();
		}
		return value2;
	}

	public int GetMarksAmount(AlignmentAxis axis)
	{
		if (!m_AxisToMarkFactsMap.TryGetValue(axis, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public IEnumerable<int> GetAlignmentRankThresholds(AlignmentAxis axis)
	{
		if (!m_AxisToMarkRanksMap.TryGetValue(axis, out var value))
		{
			yield break;
		}
		foreach (int item in value)
		{
			yield return item;
		}
	}

	public override void OnEnable()
	{
		foreach (FactsOnAlignmentMark alignmentMarkToFact in AlignmentMarkToFacts)
		{
			m_AxisToMarkRanksMap.TryAdd(alignmentMarkToFact.Axis, new List<int>());
			m_AxisToMarkRanksMap[alignmentMarkToFact.Axis].Add(alignmentMarkToFact.RankRequired);
		}
		for (int i = 0; i < AlignmentMarkToFacts.Count; i++)
		{
			FactsOnAlignmentMark factsOnAlignmentMark = AlignmentMarkToFacts[i];
			m_AxisToMarkFactsMap.TryAdd(factsOnAlignmentMark.Axis, new Dictionary<int, List<BlueprintMechanicEntityFact>>());
			m_AxisToMarkFactsMap[factsOnAlignmentMark.Axis][i] = factsOnAlignmentMark.Facts.Select((BpRef<BlueprintMechanicEntityFact> f) => f.Blueprint).ToList();
		}
		m_AlignmentStaticInfoMap = AlignmentStaticInfo.ToDictionary((AlignmentAxisStaticInfo x) => x.Axis, (AlignmentAxisStaticInfo x) => x);
	}

	public int GetMarkForRank(AlignmentAxis axis, int currentRank)
	{
		if (!m_AxisToMarkRanksMap.TryGetValue(axis, out var value))
		{
			return 0;
		}
		return value.IndexOf(value.LastOrDefault((int r) => currentRank >= r)) + 1;
	}

	public int GetRankForMark(AlignmentAxis axis, int mark)
	{
		if (mark == 0)
		{
			return 0;
		}
		if (!m_AxisToMarkRanksMap.TryGetValue(axis, out var value))
		{
			return 0;
		}
		return value[mark - 1];
	}

	public AlignmentAxisStaticInfo GetAlignmentInfo(AlignmentAxis direction)
	{
		return m_AlignmentStaticInfoMap[direction];
	}
}
