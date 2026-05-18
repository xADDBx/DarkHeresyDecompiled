using Cinemachine;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class CameraFollowTurnTask : ICameraFollowTask, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IWarhammerAttackHandler
{
	private readonly MechanicEntity m_Entity;

	public bool IsStarted { get; private set; }

	public bool CanStartBrain { get; }

	public CameraFollowTaskParams TaskParams { get; }

	public Transform Target { get; }

	public Vector3 Position { get; }

	public int Priority { get; }

	public bool IsActive { get; private set; }

	public string DebugName { get; }

	public CinemachineVirtualCamera VirtualCamera { get; private set; }

	public CameraFollowTurnTask(CameraFollowTaskParams taskParams, MechanicEntity entity, Transform target, int priority, bool canStartBrain, string debugName)
	{
		m_Entity = entity;
		TaskParams = taskParams;
		Target = target;
		Position = target.position;
		Priority = priority;
		DebugName = debugName;
		CanStartBrain = canStartBrain;
		EventBus.Subscribe(this);
	}

	public void Start(CinemachineVirtualCamera vcam)
	{
		IsStarted = true;
		VirtualCamera = vcam;
		IsActive = true;
	}

	public void End()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity != m_Entity)
		{
			IsActive = false;
		}
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.Initiator == m_Entity)
		{
			IsActive = false;
		}
	}
}
