using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.Interaction;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class InteractionProcessPart : MechanicEntityPart, IHashable, IOwlPackable<InteractionProcessPart>
{
	private readonly List<InteractionProcess> m_InteractionProcesses = new List<InteractionProcess>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionProcessPart",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool HasActiveInteraction
	{
		get
		{
			if (m_InteractionProcesses.Count > 0)
			{
				return m_InteractionProcesses.AnyItem<InteractionProcess>((InteractionProcess i) => !i.IsFinished);
			}
			return false;
		}
	}

	public void Add(InteractionProcess interactionProcess)
	{
		m_InteractionProcesses.Add(interactionProcess);
	}

	public void Remove(InteractionProcess interactionProcess)
	{
		m_InteractionProcesses.Remove(interactionProcess);
		if (m_InteractionProcesses.Count == 0)
		{
			RemoveSelf();
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
		InteractionProcessPart source = new InteractionProcessPart();
		result = Unsafe.As<InteractionProcessPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InteractionProcessPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionProcessPart>();
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
