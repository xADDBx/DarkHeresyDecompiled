using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct MechanicActorStatsCollection
{
	private readonly MechanicActor _actor;

	public MechanicActorStatsCollection(MechanicActor actor)
	{
		_actor = actor;
	}

	public MechanicActorStat GetStat(StatType type)
	{
		return new MechanicActorStat(_actor, type);
	}

	public MechanicActorStat GetAttribute(StatType type)
	{
		return new MechanicActorStat(_actor, type);
	}

	public MechanicActorStat GetSkill(StatType type)
	{
		return new MechanicActorStat(_actor, type);
	}
}
