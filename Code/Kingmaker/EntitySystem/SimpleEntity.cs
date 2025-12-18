using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class SimpleEntity : Entity, IHashable, IOwlPackable<SimpleEntity>
{
	protected SimpleEntity(IEntityViewBase view)
		: base(view.UniqueViewId, view.IsInGameBySettings)
	{
	}

	protected SimpleEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected SimpleEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected SimpleEntity()
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
