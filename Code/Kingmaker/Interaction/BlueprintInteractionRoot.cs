using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Interaction;

[TypeId("146c0886bade0a44a9c5a356d35a8775")]
public class BlueprintInteractionRoot : BlueprintScriptableObject
{
	[Serializable]
	public class GlobalInteractionSettings
	{
		[SerializeField]
		[ValidateNotNull]
		private StatType[] m_InteractOnlyByNotInteractedUnitSkills;

		public bool CheckInteractOnlyByNotInteractedUnit(StatType skill)
		{
			StatType[] interactOnlyByNotInteractedUnitSkills = m_InteractOnlyByNotInteractedUnitSkills;
			for (int i = 0; i < interactOnlyByNotInteractedUnitSkills.Length; i++)
			{
				if (interactOnlyByNotInteractedUnitSkills[i] == skill)
				{
					return true;
				}
			}
			return false;
		}
	}

	[Serializable]
	public class Referense : BlueprintReference<BlueprintInteractionRoot>
	{
	}

	[Serializable]
	public class OvertipVisualSettings
	{
		[field: SerializeField]
		public float OnHoverVisibility { get; private set; } = 1f;


		[field: SerializeField]
		public float NotHoverVisibility { get; private set; } = 0.65f;


		[field: SerializeField]
		public float DefaultSizeMaxDistance { get; private set; } = 6f;


		[field: SerializeField]
		public float ReducedSizeMinDistance { get; private set; } = 10f;


		[field: SerializeField]
		public float ReducedSizeMaxDistance { get; private set; } = 14f;


		[field: SerializeField]
		public float ReducedSizeValue { get; private set; } = 0.7f;

	}

	[SerializeField]
	[ValidateNotNull]
	private GlobalInteractionSettings m_GlobalInteractionSkillCheckSettings;

	[SerializeField]
	[ValidateNotNull]
	private GlobalInteractionSettings m_GlobalSkillCheckRestrictionSettings;

	[SerializeField]
	private int m_InteractionDCVariation = 2;

	[SerializeField]
	[InfoBox("Used to calculate result count after item destruction: Item.Cost / MagicPowerCost = `count of MagicPowerItem`")]
	private int m_MagicPowerCost = 100;

	[SerializeField]
	[InfoBox("Will drop from destroyed items to indicate their cost")]
	private BlueprintItemReference m_MagicPowerItem;

	[SerializeField]
	private PrefabLink m_DestructionFx;

	[SerializeField]
	private List<EnumIconEntry<UIInteractionType>> m_InteractionEntries = new List<EnumIconEntry<UIInteractionType>>();

	public GlobalInteractionSettings GlobalInteractionSkillCheckSettings => m_GlobalInteractionSkillCheckSettings;

	public GlobalInteractionSettings GlobalSkillCheckRestrictionSettings => m_GlobalSkillCheckRestrictionSettings;

	[field: SerializeField]
	public bool ReduceIconsSize { get; private set; } = true;


	[field: SerializeField]
	public OvertipVisualSettings MapObjectOvertipsVisualSettings { get; private set; } = new OvertipVisualSettings();


	public int InteractionDCVariation => m_InteractionDCVariation;

	public BlueprintItem MagicPowerItem => m_MagicPowerItem;

	public int MagicPowerCost => m_MagicPowerCost;

	public PrefabLink DestructionFx => m_DestructionFx;

	public Sprite GetInteractionIcon(UIInteractionType type)
	{
		return m_InteractionEntries.FirstOrDefault((EnumIconEntry<UIInteractionType> e) => e.Type == type)?.Icon;
	}
}
