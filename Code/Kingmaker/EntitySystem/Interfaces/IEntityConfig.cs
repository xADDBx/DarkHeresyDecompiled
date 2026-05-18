using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntityConfig
{
	string EntityId { get; }

	bool IsInGameBySettings { get; }

	bool CreatedAtRuntime { get; }

	Vector3 Position { get; }

	Quaternion Rotation { get; }

	float Orientation => Rotation.eulerAngles.y;

	string ViewName { get; }

	IEnumerable<IEntityPartConfig> Parts { get; }
}
