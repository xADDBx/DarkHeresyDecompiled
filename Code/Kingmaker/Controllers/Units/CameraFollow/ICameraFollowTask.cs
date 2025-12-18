using Cinemachine;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Camera;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public interface ICameraFollowTask
{
	bool IsStarted { get; }

	bool CanStartBrain { get; }

	CameraFollowTaskParams TaskParams { get; }

	[CanBeNull]
	Transform Target { get; }

	Vector3 Position { get; }

	int Priority { get; }

	bool IsActive { get; }

	CinemachineVirtualCamera VirtualCamera { get; }

	string DebugName { get; }

	void Start(CinemachineVirtualCamera vcam);

	void End();
}
