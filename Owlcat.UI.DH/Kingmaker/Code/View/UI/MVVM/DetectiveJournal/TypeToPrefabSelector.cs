using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[Serializable]
public class TypeToPrefabSelector<T>
{
	[Serializable]
	public class Entity
	{
		public BlueprintClue.UIType KeyType { get; private set; }

		public T Value { get; private set; }
	}

	public List<Entity> EntitiesWithTypes { get; private set; }

	public T DefaultEntity { get; private set; }

	public T GetEntity(BlueprintClue.UIType clueType)
	{
		Entity entity = EntitiesWithTypes.FirstOrDefault((Entity e) => e.KeyType == clueType);
		if (entity != null)
		{
			return entity.Value;
		}
		return DefaultEntity;
	}
}
