using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Scene.DetectiveServoskull;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

public sealed class DetectiveServoskullFlashlightController : IControllerTick, IController, IControllerEnable
{
	private PartyAwarenessController m_AwarenessController;

	private UnitPartFlashlight m_Part;

	private BaseUnitEntity m_Owner;

	private Func<Vector3> m_ForwardPointProvider;

	private readonly List<AbstractInteractionPart> m_PendingFlashlightInteractions = new List<AbstractInteractionPart>();

	public FlashlightState State { get; private set; }

	public Vector3 TargetPoint { get; private set; }

	private DetectiveServoskullRoot Settings => ConfigRoot.Instance.DetectiveServoskull;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerEnable.OnEnable()
	{
		m_AwarenessController = Game.Instance.Controllers.PartyAwarenessController;
	}

	public void Setup(BaseUnitEntity owner, UnitPartFlashlight part, Func<Vector3> forwardPointProvider)
	{
		m_Owner = owner;
		m_Part = part;
		m_ForwardPointProvider = forwardPointProvider;
	}

	internal void SetFlashlightPoint(Vector3 point)
	{
		UpdateObjectsAtTargetAndNearOwner(point);
	}

	void IControllerTick.Tick()
	{
		FlushPendingInteractions();
		UnitPartFlashlight part = m_Part;
		if (part == null || !part.IsFlashlightEnabled)
		{
			return;
		}
		FlashlightState updatedFlashlightState = GetUpdatedFlashlightState();
		if (updatedFlashlightState == FlashlightState.ForcedLookAtPosition)
		{
			UpdateObjectsAtTargetAndNearOwner(m_Part.ForcedLookAtPosition.Value);
		}
		else if (UtilityNet.IsControlMainCharacter())
		{
			Vector3 vector = ((updatedFlashlightState == FlashlightState.FollowingCursor) ? PointerController.WorldPositionForSimulation : m_ForwardPointProvider());
			if (vector != TargetPoint)
			{
				Game.Instance.GameCommandQueue.SetFlashlightTarget(vector);
			}
		}
	}

	private FlashlightState GetUpdatedFlashlightState()
	{
		return State = (m_Part.ForcedLookAtPosition.HasValue ? FlashlightState.ForcedLookAtPosition : ((!m_Owner.MovementAgent.IsReallyMoving) ? FlashlightState.FollowingCursor : FlashlightState.LookingTowards));
	}

	private void UpdateObjectsAtTargetAndNearOwner(Vector3 target)
	{
		TargetPoint = target;
		List<MapObjectEntity> value;
		using (CollectionPool<List<MapObjectEntity>, MapObjectEntity>.Get(out value))
		{
			foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
			{
				PartAwarenessCheck awarenessCheck = mapObject.AwarenessCheck;
				if (awarenessCheck != null)
				{
					awarenessCheck.IsRevealedByFlashlight = false;
				}
				if (!mapObject.SuppressedByFlashlight)
				{
					mapObject.SuppressedByFlashlight = true;
					value.Add(mapObject);
				}
				if (mapObject.FlashlightOwnerNear)
				{
					mapObject.FlashlightOwnerNear = false;
					value.Add(mapObject);
				}
			}
			List<Entity> collection = EntityBoundsHelper.FindEntitiesInRange(target, Settings.FlashlightRadius);
			Vector3 eyePosition = m_Part.Owner.EyePosition;
			float magnitude = new Vector2(eyePosition.x - target.x, eyePosition.z - target.z).magnitude;
			List<Entity> list = EntityBoundsHelper.FindEntitiesInRange(eyePosition, magnitude + Settings.FlashlightRadius);
			List<Entity> value2;
			using (CollectionPool<List<Entity>, Entity>.Get(out value2))
			{
				value2.AddRange(collection);
				foreach (Entity item in list)
				{
					if (IsInsideTriangularPrism(item.Position, eyePosition, target, Settings.FlashlightRadius) && !value2.Contains(item))
					{
						value2.Add(item);
					}
				}
				foreach (Entity item2 in EntityBoundsHelper.FindEntitiesInRange(m_Part.Owner.Position, Settings.FlashlightHolderRadius))
				{
					if (item2 is MapObjectEntity mapObjectEntity && !item2.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity) && !mapObjectEntity.FlashlightOwnerNear)
					{
						mapObjectEntity.FlashlightOwnerNear = true;
						value.Add(mapObjectEntity);
					}
				}
				foreach (Entity item3 in value2)
				{
					if (item3 is MapObjectEntity mapObjectEntity2 && !item3.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity2))
					{
						if (mapObjectEntity2.SuppressedByFlashlight)
						{
							mapObjectEntity2.SuppressedByFlashlight = false;
							value.Add(mapObjectEntity2);
						}
						TryTriggerAwareness(mapObjectEntity2);
					}
				}
				foreach (MapObjectEntity item4 in value)
				{
					item4.View.UpdateHighlight();
				}
			}
		}
	}

	private void TryTriggerAwareness(MapObjectEntity mapObject)
	{
		PartAwarenessCheck optional = mapObject.GetOptional<PartAwarenessCheck>();
		if (optional == null || !optional.Settings.HiddenInDarkness)
		{
			return;
		}
		optional.IsRevealedByFlashlight = true;
		AbstractInteractionPart abstractInteractionPart = mapObject.Interactions.FirstOrDefault((AbstractInteractionPart i) => i.Type == InteractionType.Flashlight);
		if (!optional.GetPassed())
		{
			m_AwarenessController.ForceUpdateMapObject(mapObject);
			if (optional.GetPassed() && abstractInteractionPart != null)
			{
				m_PendingFlashlightInteractions.Add(abstractInteractionPart);
			}
		}
		else if (optional.GetPassed() && abstractInteractionPart != null && !abstractInteractionPart.AlreadyVisited)
		{
			m_PendingFlashlightInteractions.Add(abstractInteractionPart);
		}
	}

	private void FlushPendingInteractions()
	{
		foreach (AbstractInteractionPart pendingFlashlightInteraction in m_PendingFlashlightInteractions)
		{
			UnitCommandsRunner.DirectInteract(m_Owner, pendingFlashlightInteraction);
		}
		m_PendingFlashlightInteractions.Clear();
	}

	private static bool IsInsideTriangularPrism(Vector3 point, Vector3 eye, Vector3 target, float radius)
	{
		if (point.y < eye.y - radius || point.y > eye.y + radius)
		{
			return false;
		}
		Vector2 vector = new Vector2(target.x - eye.x, target.z - eye.z);
		float magnitude = vector.magnitude;
		if (magnitude < float.Epsilon)
		{
			return false;
		}
		Vector2 rhs = vector / magnitude;
		Vector2 rhs2 = new Vector2(0f - rhs.y, rhs.x);
		Vector2 lhs = new Vector2(point.x - eye.x, point.z - eye.z);
		float num = Vector2.Dot(lhs, rhs);
		if (num < 0f || num > magnitude)
		{
			return false;
		}
		float num2 = radius * num / magnitude;
		return Mathf.Abs(Vector2.Dot(lhs, rhs2)) <= num2;
	}

	private void UpdateHighlightNearObjects()
	{
		HashSet<MapObjectEntity> hashSet = new HashSet<MapObjectEntity>();
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			if (mapObject.FlashlightOwnerNear)
			{
				mapObject.FlashlightOwnerNear = false;
				hashSet.Add(mapObject);
			}
		}
		foreach (Entity item in EntityBoundsHelper.FindEntitiesInRange(m_Part.Owner.Position, Settings.FlashlightHolderRadius))
		{
			if (item is MapObjectEntity mapObjectEntity && !item.IsInFogOfWar && m_Owner.HasLOS(mapObjectEntity) && !mapObjectEntity.FlashlightOwnerNear)
			{
				mapObjectEntity.FlashlightOwnerNear = true;
				hashSet.Add(mapObjectEntity);
			}
		}
		foreach (MapObjectEntity item2 in hashSet)
		{
			if (item2.FlashlightOwnerNear)
			{
				TryTriggerAwareness(item2);
			}
			item2.View.UpdateHighlight();
		}
	}
}
