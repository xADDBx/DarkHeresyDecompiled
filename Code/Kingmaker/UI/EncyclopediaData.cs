using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[OwlPackable(OwlPackableMode.Generate)]
public class EncyclopediaData : IHashable, IOwlPackable, IOwlPackable<EncyclopediaData>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EncyclopediaData",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_ViewedNodes", typeof(HashSet<BlueprintEncyclopediaNode>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private HashSet<BlueprintEncyclopediaNode> m_ViewedNodes { get; set; } = new HashSet<BlueprintEncyclopediaNode>();


	public IEnumerable<BlueprintEncyclopediaNode> ViewedNodes => m_ViewedNodes;

	public void MarkViewed(BlueprintEncyclopediaNode node)
	{
		if (m_ViewedNodes.Add(node))
		{
			EventBus.RaiseEvent(delegate(IEncyclopediaNodeViewedHandler h)
			{
				h.HandleEncyclopediaNodeViewed(node);
			});
		}
	}

	public bool IsViewed(BlueprintEncyclopediaNode node)
	{
		return m_ViewedNodes.Contains(node);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<BlueprintEncyclopediaNode> viewedNodes = m_ViewedNodes;
		if (viewedNodes != null)
		{
			int num = 0;
			foreach (BlueprintEncyclopediaNode item in viewedNodes)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EncyclopediaData source = new EncyclopediaData();
		result = Unsafe.As<EncyclopediaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EncyclopediaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		HashSet<BlueprintEncyclopediaNode> value = m_ViewedNodes;
		formatter.Field(0, "m_ViewedNodes", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EncyclopediaData>();
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
				m_ViewedNodes = formatter.ReadPackable<HashSet<BlueprintEncyclopediaNode>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
