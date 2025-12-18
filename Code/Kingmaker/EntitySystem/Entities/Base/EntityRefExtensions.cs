using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.EntitySystem.Entities.Base;

public static class EntityRefExtensions
{
	public static IEnumerable<Entity> Dereference(this IEnumerable<EntityRef> source)
	{
		return source.Select((EntityRef i) => (Entity)i.Entity);
	}

	public static IEnumerable<T> Dereference<T>(this IEnumerable<EntityRef<T>> source) where T : class, IEntity
	{
		return source.Select((EntityRef<T> i) => i.Entity);
	}
}
