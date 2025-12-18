using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Blueprints;

[Serializable]
[TypeId("618a7e0d54149064ab3ffa5d9057362c")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintBuff : BlueprintUnitFact, IResourcesHolder, IResourceIdsHolder, IBlueprintScanner, IBlueprintFactWithRanks
{
	[Flags]
	private enum Flags
	{
		HiddenInUi = 1,
		ShowInLogOnlyOnYourself = 2,
		StayOnDeath = 4,
		NeedsNoVisual = 8,
		DynamicDamage = 0x10,
		ShowInDialogue = 0x20,
		PriorityInUI = 0x40,
		HideInLog = 0x100,
		CriticalEffect = 0x200,
		HideInCombatTextOnAttach = 0x400
	}

	public enum InitiativeType
	{
		ByCaster,
		ByOwner,
		Current
	}

	[SerializeField]
	[EnumFlagsAsButtons]
	private Flags m_Flags;

	[SerializeField]
	private AkSwitchReference m_SoundTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_MuffledTypeSwitch;

	public StackingType Stacking;

	[ShowIf("IsStackingRank")]
	public bool ProlongWhenRankAdded;

	[ShowIf("IsHighestByPriority")]
	public ContextPropertyName PriorityProperty;

	public InitiativeType Initiative;

	[SerializeField]
	[ShowIf("HasRanks")]
	public int Ranks;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	public string[] ResourceAssetIds;

	public bool PlayOnlyFirstHitSound;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	[SerializeField]
	private bool m_Cyclical;

	public int MaxRank
	{
		get
		{
			if (!HasRanks)
			{
				return 1;
			}
			return Math.Max(1, Ranks);
		}
	}

	public AkSwitchReference SoundTypeSwitch => m_SoundTypeSwitch;

	public AkSwitchReference MuffledTypeSwitch => m_MuffledTypeSwitch;

	public bool HasRanks
	{
		get
		{
			StackingType stacking = Stacking;
			return stacking == StackingType.Rank || stacking == StackingType.Stack;
		}
	}

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public BuffUISettings BuffUISettings => base.ComponentsArray.FirstOrDefault((BlueprintComponent p) => p is BuffUISettings) as BuffUISettings;

	public bool IsHiddenInUI => HasFlag(Flags.HiddenInUi);

	public bool NeedsNoVisual => HasFlag(Flags.NeedsNoVisual);

	public bool StayOnDeath => HasFlag(Flags.StayOnDeath);

	public bool ShowInLogOnlyOnYourself => HasFlag(Flags.ShowInLogOnlyOnYourself);

	public bool DynamicDamage => HasFlag(Flags.DynamicDamage);

	public bool ShowInDialogue => HasFlag(Flags.ShowInDialogue);

	public bool IsCriticalEffect => HasFlag(Flags.CriticalEffect);

	public bool PriorityInUI => HasFlag(Flags.PriorityInUI);

	public bool HideInLog => HasFlag(Flags.HideInLog);

	public bool CriticalEffect => HasFlag(Flags.CriticalEffect);

	public bool IsHardCrowdControl => this.GetComponent<HardCrowdControlBuff>() != null;

	private bool IsHighestByPriority => Stacking == StackingType.HighestByProperty;

	private bool IsStackingRank => Stacking == StackingType.Rank;

	public bool IsDOTVisual => this.GetComponent<DOTLogic>() != null;

	public bool HasBuffOverrideUIOrder => this.GetComponent<BuffOverrideUIOrder>() != null;

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroups = m_AbilityGroups;
			return abilityGroups;
		}
	}

	public bool Cyclical => m_Cyclical;

	protected override Type GetFactType()
	{
		return typeof(Buff);
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new Buff(this, parentContext, duration, rank);
	}

	private bool HasFlag(Flags flag)
	{
		return (m_Flags & flag) != 0;
	}

	public string[] GetResourceIds()
	{
		return ResourceAssetIds;
	}

	public IEnumerable<WeakResourceLink> GetResources()
	{
		return null;
	}

	public void Scan()
	{
	}

	public void Editor_SetFXSettings(BlueprintAbilityFXSettings fxSettings)
	{
	}

	public void Editor_SetTechnicalBuffFlags()
	{
	}
}
