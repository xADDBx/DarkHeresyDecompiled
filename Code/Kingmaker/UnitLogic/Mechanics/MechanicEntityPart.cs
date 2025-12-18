using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityPart<TEntity> : EntityPart<TEntity>, IHashable, IOwlPackable<MechanicEntityPart<TEntity>> where TEntity : MechanicEntity
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityPart : MechanicEntityPart<MechanicEntity>, IHashable, IOwlPackable<MechanicEntityPart>
{
	public override Type RequiredEntityType => EntityInterfacesHelper.MechanicEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
