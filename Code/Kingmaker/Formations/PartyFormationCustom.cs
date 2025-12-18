using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Formations;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartyFormationCustom : IPartyFormation, IImmutablePartyFormation, IHashable, IOwlPackable, IOwlPackable<PartyFormationCustom>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartyFormationCustom",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Positions", typeof(Vector2[]))
		}
	};

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	private Vector2[] m_Positions { get; set; }

	public float Length => BlueprintPartyFormation.GetLength(m_Positions);

	public AbstractUnitEntity Tank => Game.Instance.Player.PartyAndPets.Get(BlueprintPartyFormation.GetTankIndex(m_Positions));

	[JsonConstructor]
	public PartyFormationCustom(Vector2[] m_positions)
	{
		m_Positions = m_positions;
	}

	public PartyFormationCustom()
	{
	}

	public Vector2 GetOffset(int index, AbstractUnitEntity _)
	{
		return PartyFormationHelper.GetOffset(m_Positions, index);
	}

	public void SetOffset(int index, AbstractUnitEntity _, Vector2 pos)
	{
		PartyFormationHelper.SetOffset(m_Positions, index, pos);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(m_Positions);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationCustom source = new PartyFormationCustom();
		result = Unsafe.As<PartyFormationCustom, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<PartyFormationCustom>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector2[] value = m_Positions;
		formatter.Field(0, "m_Positions", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationCustom>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_Positions = formatter.ReadPackable<Vector2[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
