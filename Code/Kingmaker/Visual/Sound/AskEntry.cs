using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class AskEntry
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Ask)]
	public SharedStringAsset Text;

	[AkEventReference]
	public string AkEvent;

	public float RandomWeight = 1f;

	public int ExcludeTime = 1;

	[Tooltip("bark can only trigger when ALL these flags are UNLOCKED")]
	[SerializeField]
	[FormerlySerializedAs("RequiredFlags")]
	private BlueprintUnlockableFlagReference[] m_RequiredFlags;

	[Tooltip("bark can only trigger when ALL these flags are LOCKED")]
	[SerializeField]
	[FormerlySerializedAs("ExcludedFlags")]
	private BlueprintUnlockableFlagReference[] m_ExcludedFlags;

	[Tooltip("we trigger condition met barks with higher priorities first, we fallback to lower priority only if condition executes to false")]
	[ShowIf("HasCondition")]
	public int ConditionPriority;

	[Tooltip("bark with no condition specified is always treated at zero priority")]
	public ConditionsChecker Condition;

	[NonSerialized]
	public int ExclusionCounter;

	public bool IsEmpty
	{
		get
		{
			if (Text == null)
			{
				return string.IsNullOrEmpty(AkEvent);
			}
			return false;
		}
	}

	public ReferenceArrayProxy<BlueprintUnlockableFlag> RequiredFlags
	{
		get
		{
			BlueprintReference<BlueprintUnlockableFlag>[] requiredFlags = m_RequiredFlags;
			return requiredFlags;
		}
	}

	public ReferenceArrayProxy<BlueprintUnlockableFlag> ExcludedFlags
	{
		get
		{
			BlueprintReference<BlueprintUnlockableFlag>[] excludedFlags = m_ExcludedFlags;
			return excludedFlags;
		}
	}

	public bool HasCondition => Condition?.HasConditions ?? false;

	public bool Locked
	{
		get
		{
			if (!RequiredFlags.EmptyIfNull().Any((BlueprintUnlockableFlag f) => Game.Instance.Player.UnlockableFlags.IsLocked(f)))
			{
				return ExcludedFlags.EmptyIfNull().Any((BlueprintUnlockableFlag f) => Game.Instance.Player.UnlockableFlags.IsUnlocked(f));
			}
			return true;
		}
	}
}
