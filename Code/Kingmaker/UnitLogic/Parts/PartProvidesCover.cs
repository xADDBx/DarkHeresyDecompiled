using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Components;
using Kingmaker.Controllers;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Covers;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartProvidesCover : UnitPart, IDynamicCoverProvider, IHashable, IOwlPackable<PartProvidesCover>
{
	private readonly HashSet<ProvidesCover> _sources = new HashSet<ProvidesCover>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartProvidesCover",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool IsActive => _sources.Count > 0;

	public NodeList Nodes
	{
		get
		{
			if (!base.Owner.IsInGame)
			{
				return NodeList.Empty;
			}
			return base.Owner.GetOccupiedNodes();
		}
	}

	public LosCalculations.CoverType CoverType => LosCalculations.CoverType.Cover;

	public void Retain(ProvidesCover source)
	{
		_sources.Add(source);
		Game.Instance.Controllers.ForcedCoversController.RegisterCoverProvider(this);
	}

	public void Release(ProvidesCover source)
	{
		_sources.Remove(source);
		if (_sources.Count == 0)
		{
			Game.Instance.Controllers.ForcedCoversController.UnregisterCoverProvider(this);
			RemoveSelf();
		}
	}

	protected override void OnDetach()
	{
		if (_sources.Count > 0)
		{
			Game.Instance.Controllers.ForcedCoversController.UnregisterCoverProvider(this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartProvidesCover source = new PartProvidesCover();
		result = Unsafe.As<PartProvidesCover, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<PartProvidesCover>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartProvidesCover>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
