using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Formations;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartyFormationFlashlight : IPartyFormation, IImmutablePartyFormation, IOwlPackable, IOwlPackable<PartyFormationFlashlight>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartyFormationFlashlight",
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

	public AbstractUnitEntity Tank => Game.Instance.Player.MainCharacter.ToAbstractUnitEntity();

	[JsonConstructor]
	public PartyFormationFlashlight(Vector2[] m_positions)
	{
		m_Positions = m_positions;
	}

	public PartyFormationFlashlight()
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

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationFlashlight source = new PartyFormationFlashlight();
		result = Unsafe.As<PartyFormationFlashlight, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyFormationFlashlight>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector2[] value = m_Positions;
		formatter.Field(0, "m_Positions", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationFlashlight>();
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
