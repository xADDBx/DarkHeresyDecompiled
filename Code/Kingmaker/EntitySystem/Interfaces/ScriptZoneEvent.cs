using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine.Events;

namespace Kingmaker.EntitySystem.Interfaces;

[Serializable]
public class ScriptZoneEvent : UnityEvent<BaseUnitEntity, ScriptZoneEntity>
{
}
