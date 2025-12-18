using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class CustomCluesConnections : IHashable, IOwlPackable, IOwlPackable<CustomCluesConnections>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CustomCluesConnections",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_CluesFromTo", typeof(Dictionary<BlueprintClue, List<BlueprintClue>>))
		}
	};

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Dictionary<BlueprintClue, List<BlueprintClue>> m_CluesFromTo { get; set; } = new Dictionary<BlueprintClue, List<BlueprintClue>>();


	public void AddConnectionFromTo(BlueprintClue clueFrom, BlueprintClue clueTo)
	{
		HashSet<BlueprintClue> openedConnectedClues = UtilityDetective.GetOpenedConnectedClues(clueFrom);
		HashSet<BlueprintClue> openedConnectedClues2 = UtilityDetective.GetOpenedConnectedClues(clueTo);
		if (!openedConnectedClues.Contains(clueTo) && !openedConnectedClues2.Contains(clueFrom))
		{
			if (!m_CluesFromTo.ContainsKey(clueFrom))
			{
				m_CluesFromTo.Add(clueFrom, new List<BlueprintClue>());
			}
			if (!m_CluesFromTo[clueFrom].Contains(clueTo))
			{
				m_CluesFromTo[clueFrom].Add(clueTo);
			}
		}
	}

	public void RemoveConnectionFromTo(BlueprintClue clueFrom, BlueprintClue clueTo)
	{
		if (m_CluesFromTo.ContainsKey(clueFrom) && m_CluesFromTo[clueFrom].Contains(clueTo))
		{
			m_CluesFromTo[clueFrom].Remove(clueTo);
		}
	}

	public IEnumerable<BlueprintClue> GetConnectedCluesFor(BlueprintClue clueFrom)
	{
		if (!m_CluesFromTo.TryGetValue(clueFrom, out var value))
		{
			return new List<BlueprintClue>();
		}
		return value;
	}

	public bool HasCustomConnection(BlueprintClue clueFrom, BlueprintClue clueTo)
	{
		if (!m_CluesFromTo.TryGetValue(clueFrom, out var value) || !value.Contains(clueTo))
		{
			if (m_CluesFromTo.TryGetValue(clueTo, out var value2))
			{
				return value2.Contains(clueFrom);
			}
			return false;
		}
		return true;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CustomCluesConnections source = new CustomCluesConnections();
		result = Unsafe.As<CustomCluesConnections, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CustomCluesConnections>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<BlueprintClue, List<BlueprintClue>> value = m_CluesFromTo;
		formatter.Field(0, "m_CluesFromTo", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CustomCluesConnections>();
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
				m_CluesFromTo = formatter.ReadPackable<Dictionary<BlueprintClue, List<BlueprintClue>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
