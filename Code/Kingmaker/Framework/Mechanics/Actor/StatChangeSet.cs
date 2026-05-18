using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct StatChangeSet
{
	private readonly ulong _mask;

	public bool IsEmpty => _mask == 0;

	internal StatChangeSet(ulong mask)
	{
		_mask = mask;
	}

	public bool Contains(StatType stat)
	{
		return (_mask & (ulong)(1L << (int)stat)) != 0;
	}
}
