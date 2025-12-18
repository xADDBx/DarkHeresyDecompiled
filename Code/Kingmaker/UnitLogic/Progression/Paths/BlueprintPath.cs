using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[TypeId("3eb928a0df1049099c82edc91d03d8da")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public abstract class BlueprintPath : BlueprintFeature
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintPath>
	{
	}

	[Serializable]
	public class RankEntry
	{
		[SerializeField]
		[ItemNotNull]
		private BpRef<BlueprintFeature>[] m_Features = new BpRef<BlueprintFeature>[0];

		[SerializeField]
		[ItemNotNull]
		private BpRef<BlueprintSelection>[] m_Selections = new BpRef<BlueprintSelection>[0];

		public BpRefArray<BlueprintFeature> Features => m_Features;

		public BpRefArray<BlueprintSelection> Selections => m_Selections;
	}

	[InfoBox("Ranks must be equal to RankEntries.Length")]
	[ArrayElementNamePrefix("Rank", true)]
	public RankEntry[] RankEntries = new RankEntry[0];

	[CanBeNull]
	public RankEntry GetRankEntry(int level)
	{
		return RankEntries.Get(level - 1);
	}
}
