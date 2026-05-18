using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class BuffingAreaPart : InteractionPart<BuffingAreaSettings>, IHashable, IOwlPackable<BuffingAreaPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "BuffingAreaPart",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool))
		}
	};

	protected override void OnInteract(BaseUnitEntity user)
	{
		GameHelper.ApplyBuff(user, base.Settings.Buff, base.Settings.BuffDuration);
		PFLog.Default.Log("BuffApplied");
	}

	public override void OnUnitLeftProximity(BaseUnitEntity unitEntityData)
	{
		GameHelper.RemoveBuff(unitEntityData, base.Settings.Buff);
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
		BuffingAreaPart source = new BuffingAreaPart();
		result = Unsafe.As<BuffingAreaPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<BuffingAreaPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<BuffingAreaPart>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
