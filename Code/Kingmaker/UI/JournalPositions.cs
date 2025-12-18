using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class JournalPositions<T> : IHashable, IOwlPackable, IOwlPackable<JournalPositions<T>> where T : BlueprintCaseItem
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "JournalPositions",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Positions", typeof(Dictionary<T, Vector2>))
		}
	};

	private static IOutputFormatter.FieldDelegate<Dictionary<T, Vector2>> m_Serializer_Dictionary_ = null;

	private static IInputFormatter.FieldDelegate<Dictionary<T, Vector2>> m_Deserializer_Dictionary_ = null;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Dictionary<T, Vector2> m_Positions { get; set; } = new Dictionary<T, Vector2>();


	public bool HasPositionFor(T caseItem)
	{
		if (caseItem != null)
		{
			return m_Positions.ContainsKey(caseItem);
		}
		return false;
	}

	public bool TryGetPositionsFor(T caseItem, out Vector2 position)
	{
		return m_Positions.TryGetValue(caseItem, out position);
	}

	public void UpdatePositionFor(T caseItem, Vector2 position)
	{
		if (!m_Positions.TryAdd(caseItem, position))
		{
			m_Positions[caseItem] = position;
		}
	}

	public void TryRemovePositionFor(T caseItem)
	{
		if (m_Positions.ContainsKey(caseItem))
		{
			m_Positions.Remove(caseItem);
		}
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		JournalPositions<T> source = new JournalPositions<T>();
		result = Unsafe.As<JournalPositions<T>, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<JournalPositions<T>>(OwlPackTypeInfo);
		OutputFormatter.CreateFieldDelegate(formatter, ref m_Serializer_Dictionary_);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<T, Vector2> value = m_Positions;
		formatter.Field(0, "m_Positions", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		InputFormatter.CreateFieldDelegate(formatter, ref m_Deserializer_Dictionary_);
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<JournalPositions<T>>();
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
				m_Positions = formatter.ReadPackable<Dictionary<T, Vector2>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
