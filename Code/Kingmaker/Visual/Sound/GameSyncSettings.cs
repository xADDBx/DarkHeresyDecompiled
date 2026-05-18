using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;

namespace Kingmaker.Visual.Sound;

[Serializable]
public abstract class GameSyncSettings : Element
{
	public abstract void Sync(MechanicEntity entity, IEvalContext context, uint playingId);
}
