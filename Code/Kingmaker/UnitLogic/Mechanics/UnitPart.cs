using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class UnitPart : BaseUnitPart<UnitEntity>, IHashable, IOwlPackable<UnitPart>
{
	public override Type RequiredEntityType => EntityInterfacesHelper.UnitEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
