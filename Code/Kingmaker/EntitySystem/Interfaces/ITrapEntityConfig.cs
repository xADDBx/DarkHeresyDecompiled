using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.Traps;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ITrapEntityConfig : IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	bool IsNotScriptZoneTrigger { get; }

	string NameInLog { get; }

	Vector3? ActorPosition { get; }

	Vector3 TargetPosition { get; }

	EntityRef<ScriptZoneEntity> ScriptZone { get; }

	EntityRef<TrapObjectData> Device { get; }

	EntityRef<TrapObjectData> LinkedTrap { get; }

	EntityRef<MapObjectEntity> TrappedObject { get; }
}
