using System;
using Cinemachine;
using Kingmaker.Blueprints.Camera;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class CameraFollowTask : ICameraFollowTask
{
	private TimeSpan m_LifeTime;

	private TimeSpan m_FinishTime;

	public bool IsStarted { get; private set; }

	public bool CanStartBrain { get; }

	public CameraFollowTaskParams TaskParams { get; }

	public Transform Target { get; }

	public Vector3 Position { get; }

	public int Priority { get; }

	public CinemachineVirtualCamera VirtualCamera { get; private set; }

	public string DebugName { get; }

	public bool IsActive => Game.Instance.Controllers.TimeController.RealTime < m_FinishTime;

	public CameraFollowTask(CameraFollowTaskParams taskParams, Vector3 position, int priority, bool canStartBrain, string debugName)
	{
		TaskParams = taskParams;
		Position = position;
		Priority = priority;
		DebugName = debugName;
		CanStartBrain = canStartBrain;
		m_LifeTime = TimeSpan.FromSeconds(taskParams.CameraObserveTime + taskParams.BlendSettings.BlendTime);
	}

	public CameraFollowTask(CameraFollowTaskParams taskParams, Transform target, int priority, bool canStartBrain, string debugName)
	{
		TaskParams = taskParams;
		Target = target;
		Position = target.position;
		Priority = priority;
		DebugName = debugName;
		CanStartBrain = canStartBrain;
		m_LifeTime = TimeSpan.FromSeconds(taskParams.CameraObserveTime + taskParams.BlendSettings.BlendTime);
	}

	public void Start(CinemachineVirtualCamera vcam)
	{
		IsStarted = true;
		VirtualCamera = vcam;
		m_FinishTime = Game.Instance.Controllers.TimeController.RealTime + m_LifeTime;
	}

	public void End()
	{
	}
}
