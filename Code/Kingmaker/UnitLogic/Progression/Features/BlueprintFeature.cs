using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features;

[Serializable]
[TypeId("cb208b98ceacca84baee15dba53b1979")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintFeature : BlueprintFeatureBase, IBlueprintFactWithRanks
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintFeature>
	{
	}

	public enum FeatureType
	{
		Offense,
		Defense,
		Support,
		Universal,
		Archetype,
		Origin,
		Warp,
		LevelUpFeature,
		Talent,
		Modifier,
		Specialization,
		Homeworld,
		Background
	}

	private NameModifier[] m_NameModifiersCache;

	private DescriptionModifier[] m_DescriptionModifiersCache;

	[NotNull]
	public static List<BlueprintFeature> EmptyList = new List<BlueprintFeature>();

	public int Ranks = 1;

	public PrerequisitesList Prerequisites = new PrerequisitesList();

	public List<FeatureType> FeatureTypes = new List<FeatureType>();

	public TalentIconInfo TalentIconInfo = new TalentIconInfo();

	[SerializeField]
	private LocalizedString m_Acronym;

	public bool ShowInDialogue;

	public bool IsStarshipFeature;

	public string ForceSetNameForItemTooltip
	{
		get
		{
			AddForceSetName component = this.GetComponent<AddForceSetName>();
			if (component != null)
			{
				return component.ForceName;
			}
			return string.Empty;
		}
	}

	public override string Name
	{
		get
		{
			if (m_NameModifiersCache == null)
			{
				m_NameModifiersCache = this.GetComponents<NameModifier>().ToArray();
			}
			string text = base.Name;
			NameModifier[] nameModifiersCache = m_NameModifiersCache;
			for (int i = 0; i < nameModifiersCache.Length; i++)
			{
				text = nameModifiersCache[i].Modify(text);
			}
			return text;
		}
	}

	public override string Description
	{
		get
		{
			if (m_DescriptionModifiersCache == null)
			{
				m_DescriptionModifiersCache = this.GetComponents<DescriptionModifier>().ToArray();
			}
			string text = base.Description;
			DescriptionModifier[] descriptionModifiersCache = m_DescriptionModifiersCache;
			for (int i = 0; i < descriptionModifiersCache.Length; i++)
			{
				text = descriptionModifiersCache[i].Modify(text);
			}
			return text;
		}
	}

	public string Acronym => m_Acronym?.Text;

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new Feature(this, parentContext);
	}
}
