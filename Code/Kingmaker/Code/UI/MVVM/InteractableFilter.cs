using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class InteractableFilter
{
	private BaseUnitEntity m_Unit;

	public void Reset(BaseUnitEntity unit)
	{
		m_Unit = unit;
	}

	public bool IsMatch(Entity entity)
	{
		GameObject gameObject = entity?.View?.GO;
		if (gameObject == null || !gameObject.activeInHierarchy)
		{
			return false;
		}
		float maxRadius = 6.35f;
		if (!CheckUnit(entity, ref maxRadius) && !CheckInteraction(entity, ref maxRadius) && !CheckTransition(entity, ref maxRadius))
		{
			return false;
		}
		Vector3 rhs = entity.Position - m_Unit.Position;
		if (rhs.sqrMagnitude > maxRadius * maxRadius)
		{
			return false;
		}
		if (Vector3.Dot(m_Unit.Rotation * Vector3.forward, rhs) < 0f)
		{
			return false;
		}
		return true;
	}

	private bool CheckUnit(Entity entity, ref float maxRadius)
	{
		if (!(entity is AbstractUnitEntity abstractUnitEntity))
		{
			return false;
		}
		if (abstractUnitEntity.IsInFogOfWar)
		{
			return false;
		}
		IUnitInteraction unitInteraction = abstractUnitEntity.GetOptional<PartUnitInteractions>()?.SelectClickInteraction(m_Unit);
		bool flag = unitInteraction != null;
		bool isDeadAndHasLoot = abstractUnitEntity.IsDeadAndHasLoot;
		if (!flag && !isDeadAndHasLoot)
		{
			return false;
		}
		if (flag)
		{
			maxRadius += unitInteraction.Distance;
		}
		else if (isDeadAndHasLoot)
		{
			maxRadius += 4f;
		}
		return true;
	}

	private bool CheckInteraction(Entity entity, ref float maxRadius)
	{
		GameObject gO = entity.View.GO;
		if (!gO.TryGetComponent<IInteractionComponent>(out var component))
		{
			return false;
		}
		if (!ClickMapObjectHandler.HasAvailableInteractions(gO))
		{
			return false;
		}
		maxRadius += component.ProximityRadius;
		if (component is DisableTrapInteractionComponent)
		{
			maxRadius += 4f;
		}
		return true;
	}

	private bool CheckTransition(Entity entity, ref float maxRadius)
	{
		AreaTransitionPart optional = entity.GetOptional<AreaTransitionPart>();
		if (optional != null)
		{
			return !optional.CheckRestrictions(m_Unit);
		}
		return false;
	}
}
