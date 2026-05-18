using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IDestructibleEntityConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	BlueprintDestructibleObject Blueprint { get; }

	bool CanBeAttackedDirectly { get; }

	Rect Bounds { get; }

	bool DisableAutoHit { get; }

	int HitChanceModifier { get; }
}
