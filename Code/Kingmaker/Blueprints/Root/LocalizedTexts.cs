using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Gameplay.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("8dbf70f594a3d4546b4790f79c4eb49c")]
public class LocalizedTexts : BlueprintScriptableObject
{
	[ValidateNotNull]
	public GlossaryStrings[] Glossaries;

	[ValidateNotNull]
	public StatsStrings Stats;

	[ValidateNotNull]
	public ItemsFilterStrings ItemsFilter;

	[ValidateNotNull]
	public GameLogStrings GameLog;

	[ValidateNotNull]
	public UIStrings UserInterfacesText;

	[ValidateNotNull]
	public AbilityTypeString AbilityTypes;

	[ValidateNotNull]
	public WeaponRangeTypeString WeaponRangeTypes;

	[ValidateNotNull]
	public WarningNotificationString WarningNotification;

	[ValidateNotNull]
	public AbilityTargetStrings AbilityTargets;

	[ValidateNotNull]
	public AbilityRangeStrings AbilityTargetRanges;

	[ValidateNotNull]
	public DescriptorTypeStrings Descriptors;

	[ValidateNotNull]
	public ItemsStrings Items;

	[ValidateNotNull]
	public ReasonStrings Reasons;

	[ValidateNotNull]
	public PreciseAttackStrings PreciseAttack;

	[ValidateNotNull]
	public PowerBalanceStrings PowerBalance;

	[ValidateNotNull]
	public WeaponSubCategoryString WeaponSubCategories;

	[ValidateNotNull]
	public DamageTypeStrings DamageTypes;

	[ValidateNotNull]
	public AttackTypeString AttackTypes;

	[ValidateNotNull]
	public MoralePhasesStrings MoralePhases;

	[ValidateNotNull]
	public MoralePhasesStrings MoralePhaseTitles;

	[FormerlySerializedAs("SpellDescriptorConditions")]
	public ConditionsString UnitConditions;

	[ValidateNotNull]
	public CalculatedPrerequisiteStrings CalculatedPrerequisites;

	[ValidateNotNull]
	public CasterRestrictionsStrings CasterRestrictionsStrings;

	public LocalizedString LockedContainer;

	public LocalizedString UnlockedContainer;

	public LocalizedString LockedwithKey;

	public LocalizedString UnlockedWithKey;

	public LocalizedString TrapCanNotBeDisarmedDirectly;

	public LocalizedString NeedSupplyPrefix;

	public LocalizedString AccessDenied;

	public LocalizedString AccessReceived;

	[InfoBox("Interact only with {text}")]
	public LocalizedString InteractOnlyWithTool;

	public static LocalizedTexts Instance => ConfigRoot.Instance.LocalizedTexts;
}
