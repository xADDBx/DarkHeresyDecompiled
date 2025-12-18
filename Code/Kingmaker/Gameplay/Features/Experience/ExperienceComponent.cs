using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Experience;

[AllowedOn(typeof(BlueprintTrap))]
[AllowedOn(typeof(BlueprintClue))]
[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("011e862b513000f4bae31886f3489ace")]
public sealed class ExperienceComponent : BlueprintComponent, IExperienceSettings
{
	[SerializeField]
	private bool _overrideType;

	[SerializeField]
	[ShowIf("_overrideType")]
	private ExperienceType _type;

	[SerializeField]
	private bool _overrideCR;

	[SerializeField]
	[ShowIf("_overrideCR")]
	private int _overrideCRValue;

	[SerializeField]
	[ShowIf("IsQuest")]
	private bool _isMainQuest;

	private bool IsQuest
	{
		get
		{
			ExperienceType type = Type;
			return type == ExperienceType.Quest || type == ExperienceType.MainQuest;
		}
	}

	public int? OverrideValue => null;

	public int? OverrideCR
	{
		get
		{
			if (!_overrideCR)
			{
				return null;
			}
			return _overrideCRValue;
		}
	}

	public ExperienceType Type
	{
		get
		{
			if (!_overrideType)
			{
				return GetTypeByOwnerBlueprint();
			}
			return _type;
		}
	}

	private ExperienceType GetTypeByOwnerBlueprint()
	{
		BlueprintScriptableObject ownerBlueprint = base.OwnerBlueprint;
		if (!(ownerBlueprint is BlueprintUnit))
		{
			if (!(ownerBlueprint is BlueprintQuest) && !(ownerBlueprint is BlueprintQuestObjective))
			{
				if (!(ownerBlueprint is BlueprintTrap))
				{
					if (ownerBlueprint is BlueprintClue)
					{
						return ExperienceType.Investigation;
					}
					throw new ArgumentOutOfRangeException("OwnerBlueprint", base.OwnerBlueprint, null);
				}
				return ExperienceType.SkillCheck;
			}
			return _isMainQuest ? ExperienceType.MainQuest : ExperienceType.Quest;
		}
		return ExperienceType.Encounter;
	}

	public string GetDescription()
	{
		return string.Format("XP (CR={0}, {1})", OverrideCR?.ToString() ?? "Area", Type);
	}
}
