using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class SimpleEntity : Entity, IHashable, IOwlPackable<SimpleEntity>
{
	protected SimpleEntity(IEntityConfig config)
		: base(config)
	{
	}

	protected SimpleEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView CreateViewForData()
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
