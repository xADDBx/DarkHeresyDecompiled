using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class UnitGroupCommandParams : UnitCommandParams, IOwlPackable<UnitGroupCommandParams>
{
	[JsonProperty]
	public readonly Guid GroupGuid;

	[JsonProperty]
	[NotNull]
	public readonly List<EntityRef<BaseUnitEntity>> Units;

	[JsonConstructor]
	protected UnitGroupCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitGroupCommandParams(Guid groupGuid, [NotNull] List<EntityRef<BaseUnitEntity>> units, [CanBeNull] TargetWrapper target)
		: base(target)
	{
		GroupGuid = groupGuid;
		Units = units;
	}
}
public abstract class UnitGroupCommandParams<T> : UnitGroupCommandParams where T : UnitGroupCommand
{
	[JsonConstructor]
	protected UnitGroupCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitGroupCommandParams(Guid groupGuid, [NotNull] List<EntityRef<BaseUnitEntity>> units, [CanBeNull] TargetWrapper target)
		: base(groupGuid, units, target)
	{
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return (AbstractUnitCommand)Activator.CreateInstance(typeof(T), this);
	}

	public new T CreateCommand()
	{
		return (T)CreateCommandInternal();
	}
}
