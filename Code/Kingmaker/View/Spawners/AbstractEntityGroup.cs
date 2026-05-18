using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class AbstractEntityGroup : SimpleEntity, IHashable, IOwlPackable<AbstractEntityGroup>
{
	private EntityRef[] _members = Array.Empty<EntityRef>();

	public IEnumerable<Entity> Members => _members.Select((EntityRef i) => (Entity)i.Entity).NotNull();

	protected AbstractEntityGroup(IEntityGroupConfig config)
		: base(config)
	{
	}

	protected AbstractEntityGroup(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnSetConfig(IEntityConfig entityConfig)
	{
		IEntityGroupConfig entityGroupConfig = (IEntityGroupConfig)entityConfig;
		_members = entityGroupConfig.MemberIds.Select((string id) => new EntityRef(id)).ToArray();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
