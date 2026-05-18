using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public interface IAbstractUnitEntity : IMechanicEntity, IEntity, IDisposable
{
	bool IsExtra { get; }

	string CharacterName { get; }

	Gender Gender { get; }

	float GetOrientationTo(Vector3 position);

	void TurnTo(Vector3 point);
}
