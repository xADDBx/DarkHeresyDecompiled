using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public class DamagePredictionCache
{
	private AbilityData m_Ability;

	private MechanicEntity m_Unit;

	private Vector3 m_Position;

	private BlueprintBodyPart m_BodyPart;

	private DamagePredictionData m_DamagePredictionCached;

	public DamagePredictionData Get(AbilityData ability, MechanicEntity unit)
	{
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(ability.Caster);
		if ((object)m_Ability == ability && m_Unit == unit && m_BodyPart == ability.PreciseBodyPart && (m_Position - desiredPosition).sqrMagnitude < 0.0001f)
		{
			return m_DamagePredictionCached;
		}
		m_Ability = ability;
		m_Unit = unit;
		m_Position = desiredPosition;
		m_BodyPart = ability.PreciseBodyPart;
		m_DamagePredictionCached = ability.GetDamagePrediction(unit, desiredPosition);
		return m_DamagePredictionCached;
	}

	public void Invalidate()
	{
		m_Ability = null;
	}
}
