using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Components.Etudes;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartMaxPartySize : EntityPart<Player>, IHashable, IOwlPackable<PartMaxPartySize>
{
	private readonly struct Entry
	{
		private readonly EntityFactRef<Etude> _etudeRef;

		private readonly EtudeBracketSetMaxPartySize _component;

		public int MaxSize => _component.MaxPartySize;

		public Entry(Etude etude, EtudeBracketSetMaxPartySize component)
		{
			_etudeRef = etude;
			_component = component;
		}

		public bool IsFrom(Etude etude, EtudeBracketSetMaxPartySize component)
		{
			if (_etudeRef == etude)
			{
				return _component == component;
			}
			return false;
		}
	}

	private readonly List<Entry> m_Entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMaxPartySize",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public int Value
	{
		get
		{
			if (m_Entries.Count <= 0)
			{
				return 6;
			}
			List<Entry> entries = m_Entries;
			return entries[entries.Count - 1].MaxSize;
		}
	}

	public void Register(Etude etude, EtudeBracketSetMaxPartySize component)
	{
		if (m_Entries.Exists((Entry e) => e.IsFrom(etude, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
		m_Entries.Add(new Entry(etude, component));
	}

	public void Unregister(Etude etude, EtudeBracketSetMaxPartySize component)
	{
		m_Entries.RemoveAll((Entry e) => e.IsFrom(etude, component));
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
		PartMaxPartySize source = new PartMaxPartySize();
		result = Unsafe.As<PartMaxPartySize, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMaxPartySize>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMaxPartySize>();
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
