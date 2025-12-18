using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class BaseUnitPart<TEntity> : MechanicEntityPart<TEntity>, IHashable, IOwlPackable<BaseUnitPart<TEntity>> where TEntity : BaseUnitEntity
{
	public override Type RequiredEntityType => EntityInterfacesHelper.BaseUnitEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class BaseUnitPart : BaseUnitPart<BaseUnitEntity>, IHashable, IOwlPackable<BaseUnitPart>
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
