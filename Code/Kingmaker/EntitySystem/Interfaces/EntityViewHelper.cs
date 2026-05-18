using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public static class EntityViewHelper
{
	public static IEntityView Or([CanBeNull] this IEntityView we, IEntityView defaultValue)
	{
		if (we == null)
		{
			return defaultValue;
		}
		if (!((we as UnityEngine.Object) ?? throw new InvalidOperationException($"Object {we} of unknown type {we.GetType()}")))
		{
			return defaultValue;
		}
		return we;
	}

	public static TEntity GetEntity<TEntity>(this IEntityConfig view) where TEntity : class, IEntity
	{
		return EntityService.Instance.GetEntity<TEntity>(view.EntityId);
	}
}
