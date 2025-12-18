using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.Framework.VO;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionSkillCheckSettings : InteractionSettings, IBarkSource
{
	public enum FakeType
	{
		None,
		FakeSuccess,
		FakeFailure
	}

	public enum PenaltyType
	{
		None,
		[Obsolete("Not Used")]
		Damage,
		Debuff
	}

	public class SkillCheckStatOrderAttribute : EnumOrderAttribute
	{
		private static readonly Enum[] s_order = new Enum[19]
		{
			StatType.Unknown,
			StatType.SkillAthletics,
			StatType.SkillAwareness,
			StatType.SkillDemolition,
			StatType.SkillDiplomacy,
			StatType.SkillInterrogation,
			StatType.SkillIntimidation,
			StatType.SkillLoreHeresy,
			StatType.SkillLoreWarp,
			StatType.SkillLoreXenos,
			StatType.SkillMedicae,
			StatType.SkillMettle,
			StatType.SkillMobility,
			StatType.SkillReflexes,
			StatType.SkillResistance,
			StatType.SkillSleightOfHand,
			StatType.SkillTechUse,
			StatType.SkillTenacity,
			StatType.SkillWits
		};

		public override Enum[] Order => s_order;
	}

	[Space(10f)]
	[SkillCheckStatOrder]
	public StatType Skill;

	[ShowIf("CanUseWithoutSupply")]
	public bool NeedSupply = true;

	[SkillCheckActualDifficulty]
	public SkillCheckDifficulty Difficulty = SkillCheckDifficulty.Normal;

	[ShowIf("DifficultyIsCustom")]
	public int DC;

	[ShowIf("DifficultyIsCustom")]
	public ViewDCModifier[] DCModifiers = new ViewDCModifier[0];

	public FakeType FakeResult;

	[Space(10f)]
	public bool HideDC;

	[Space(10f)]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset DisplayName;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset ShortDescription;

	[HideIf("DisableAfterUse")]
	public bool OnlyCheckOnce;

	[HideIf("OnlyCheckOnce")]
	public bool CheckConditionsOnEveryInteraction;

	[ShowIf("OnlyCheckOnce")]
	public bool TriggerActionsEveryClick;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset DisplayNameAfterUse;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset ShortDescriptionPassed;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Name)]
	public SharedStringAsset ShortDescriptionFailed;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public bool IsPartyCheck;

	[Space(20f)]
	public bool FadeOnSuccess;

	public bool FadeOnFail;

	[Tooltip("Mettle - Mind Crit,\n Athletic - Arms Crit,\n Mobility - Legs Crit,\n Resistance - Torso crit")]
	public PenaltyType PenaltyForFailedSkillCheck;

	public bool ApplyPenaltyAfterFade = true;

	[Space(10f)]
	[CanBeNull]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public SharedStringAsset CheckPassedBark;

	[CanBeNull]
	[ShowCreator]
	public ActionsReference CheckPassedActions;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnSuccess")]
	private BlueprintAreaEnterPointReference m_TeleportOnSuccess;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnFail")]
	private BlueprintAreaEnterPointReference m_TeleportOnFail;

	[Space(10f)]
	[CanBeNull]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public SharedStringAsset CheckFailBark;

	[CanBeNull]
	[ShowCreator]
	public ActionsReference CheckFailedActions;

	[ShowCreator]
	[Space(10f)]
	[CanBeNull]
	public ConditionsReference Condition;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;

	public override bool ShouldShowAdditionalCombatObjective => true;

	public bool CanUseWithoutSupply
	{
		get
		{
			StatType skill = Skill;
			return skill == StatType.SkillDemolition || skill == StatType.SkillLoreXenos || skill == StatType.SkillTechUse;
		}
	}

	public BlueprintAreaEnterPoint TeleportOnSuccess => m_TeleportOnSuccess?.Get();

	public BlueprintAreaEnterPoint TeleportOnFail => m_TeleportOnFail?.Get();

	private bool DifficultyIsCustom => Difficulty == SkillCheckDifficulty.Custom;

	public IEnumerable<LocalizedString> Barks => new LocalizedString[2] { ShortDescriptionPassed.String, ShortDescriptionFailed.String };

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField v) => v.Guid).ToList();

	public bool Spammable => IsSpammable;

	public int GetDC()
	{
		if (Difficulty == SkillCheckDifficulty.AutoPass)
		{
			return 0;
		}
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		if (DCModifiers == null)
		{
			return DC;
		}
		int num = DC;
		ViewDCModifier[] dCModifiers = DCModifiers;
		foreach (ViewDCModifier viewDCModifier in dCModifiers)
		{
			if (viewDCModifier != null && viewDCModifier.Conditions?.Get()?.Check() == true)
			{
				num += viewDCModifier.Mod;
			}
		}
		return num;
	}
}
