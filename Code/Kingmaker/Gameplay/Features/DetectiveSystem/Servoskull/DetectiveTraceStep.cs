using System;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
public class DetectiveTraceStep
{
	public Vector3 Position;

	public Quaternion Rotation;

	public DetectiveTraceStep(Vector3 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
	}
}
