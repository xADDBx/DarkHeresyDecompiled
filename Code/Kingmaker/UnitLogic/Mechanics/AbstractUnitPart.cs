using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractUnitPart<TEntity> : MechanicEntityPart<TEntity>, IHashable, IOwlPackable<AbstractUnitPart<TEntity>> where TEntity : AbstractUnitEntity
{
	public override Type RequiredEntityType => EntityInterfacesHelper.AbstractUnitEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractUnitPart : AbstractUnitPart<AbstractUnitEntity>, IHashable, IOwlPackable<AbstractUnitPart>
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
