using UnityEngine.Pool;

namespace Kingmaker.Framework.Mechanics.Actor;

public static class StatModifierCollectorPool
{
	private const int DefaultCapacity = 8;

	private const int MaxSize = 16;

	private static readonly ObjectPool<StatModifierCollector> _Pool = new ObjectPool<StatModifierCollector>(() => new StatModifierCollector(), delegate(StatModifierCollector c)
	{
		c.Clear();
	}, null, null, collectionCheck: true, 8, 16);

	public static PooledObject<StatModifierCollector> Get(out StatModifierCollector collector)
	{
		return _Pool.Get(out collector);
	}
}
