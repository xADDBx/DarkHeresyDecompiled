using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Code.View.UI.MVVM;

[Serializable]
public class EnumToObjectSelector<TEnum, TObj> where TEnum : Enum
{
	[Serializable]
	public class Entity
	{
		public TEnum KeyType { get; private set; }

		public TObj Value { get; private set; }
	}

	[SerializeField]
	private bool m_HasDefaultEntity = true;

	public TObj DefaultEntity { get; private set; }

	public List<Entity> EntitiesWithTypes { get; private set; }

	public TObj GetEntity(TEnum enumType)
	{
		Entity entity = EntitiesWithTypes.FirstOrDefault((Entity e) => e.KeyType.Equals(enumType));
		if (entity != null)
		{
			return entity.Value;
		}
		if (!m_HasDefaultEntity)
		{
			return default(TObj);
		}
		return DefaultEntity;
	}
}
