using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CompanionInParty")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("d2f424beb5ace314887e9cc946b68dfa")]
public class IsCompanionInParty : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("m_companion")]
	private BlueprintUnitReference m_Companion;

	[Tooltip("Зарекручен и находится в активной партии")]
	public bool MatchWhenActive = true;

	[Tooltip("Зарекручен, находится в партии, но не управляется игроком в данный момент (стоит на месте, где находился в момент детача, пропадает из панели партии)")]
	public bool MatchWhenDetached;

	[Tooltip("Зарекручен, но в данный момент не находится в активной партии")]
	public bool MatchWhenRemote;

	[Tooltip("Анрекручен, т.е. удален из ростера")]
	public bool MatchWhenEx;

	[ShowIf("MatchWhenEx")]
	[Tooltip("Анрекручен как InReserve")]
	public bool MatchExInReserve;

	[ShowIf("MatchWhenEx")]
	[Tooltip("Анрекручен как Kicked")]
	public bool MatchExKicked;

	[ShowIf("MatchWhenEx")]
	[Tooltip("Анрекручен как Dead")]
	public bool MatchExDead;

	public BlueprintUnit Companion
	{
		get
		{
			return m_Companion?.Get();
		}
		set
		{
			m_Companion = value.ToReference<BlueprintUnitReference>();
		}
	}

	private bool IsCompanion(BlueprintUnit unit)
	{
		if (!m_Companion.Is(unit))
		{
			if (unit.PrototypeLink != null)
			{
				return IsCompanion((BlueprintUnit)unit.PrototypeLink);
			}
			return false;
		}
		return true;
	}

	protected override bool CheckCondition()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (!IsCompanion(allCharacter.Blueprint))
			{
				continue;
			}
			UnitPartCompanion optional = allCharacter.GetOptional<UnitPartCompanion>();
			CompanionState companionState = optional?.State ?? CompanionState.None;
			CompanionExState companionExState = optional?.ExState ?? CompanionExState.InReserve;
			bool flag = !MatchExInReserve && !MatchExKicked && !MatchExDead;
			switch (companionState)
			{
			case CompanionState.InParty:
				if (!MatchWhenActive)
				{
					continue;
				}
				break;
			case CompanionState.Remote:
				if (!MatchWhenRemote)
				{
					continue;
				}
				break;
			case CompanionState.ExCompanion:
				if ((!MatchWhenEx || companionExState != 0 || !MatchExInReserve) && (!MatchWhenEx || companionExState != CompanionExState.Kicked || !MatchExKicked) && (!MatchWhenEx || companionExState != CompanionExState.Dead || !MatchExDead) && !(MatchWhenEx && flag))
				{
					continue;
				}
				break;
			case CompanionState.InPartyDetached:
				if (!MatchWhenDetached)
				{
					continue;
				}
				break;
			default:
				continue;
			}
			return true;
		}
		return false;
	}

	protected override string GetConditionCaption()
	{
		return $"Is companion in party ({Companion})";
	}
}
