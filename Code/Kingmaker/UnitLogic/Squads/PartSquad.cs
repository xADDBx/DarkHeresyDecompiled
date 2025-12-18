using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Squads;

[OwlPackable(OwlPackableMode.Generate)]
public class PartSquad : BaseUnitPart, IHashable, IOwlPackable<PartSquad>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Id;

	private UnitSquad m_Squad;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartSquad",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Id", typeof(string))
		}
	};

	public UnitSquad Squad => UpdateSquad();

	public bool IsInSquad => Squad != null;

	public int Count => Squad?.Count ?? 0;

	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			Drop();
			m_Id = value;
			UpdateSquad();
		}
	}

	[CanBeNull]
	public BaseUnitEntity Leader => Squad.Leader;

	public bool IsLeader => Leader == base.Owner;

	public ReadonlyList<UnitReference> Units => Squad.Units;

	protected override void OnDetach()
	{
		Drop();
	}

	private UnitSquad UpdateSquad()
	{
		if (string.IsNullOrEmpty(Id))
		{
			return null;
		}
		if (m_Squad != null)
		{
			return m_Squad;
		}
		m_Squad = UnitSquad.GetOrCreate(Id);
		m_Squad.Add(base.Owner);
		return m_Squad;
	}

	private void Drop()
	{
		m_Squad?.Remove(base.Owner);
		m_Squad = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_Id);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartSquad source = new PartSquad();
		result = Unsafe.As<PartSquad, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartSquad>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Id", ref m_Id, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartSquad>();
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
				m_Id = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
