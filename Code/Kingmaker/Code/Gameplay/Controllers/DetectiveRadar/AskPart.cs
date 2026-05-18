using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

[OwlPackable(OwlPackableMode.Generate)]
public class AskPart : EntityPartWithConfig<AskSettings>, IHashable, IOwlPackable<AskPart>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_Triggered;

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan m_TriggeredTime;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AskPart",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("m_Triggered", typeof(bool)),
			new FieldInfo("m_TriggeredTime", typeof(TimeSpan))
		}
	};

	public bool CanBeTriggered
	{
		get
		{
			if (!base.Owner.IsInGame)
			{
				return false;
			}
			if (base.Settings.Once)
			{
				return !m_Triggered;
			}
			if (IsOnCooldown)
			{
				return false;
			}
			return true;
		}
	}

	public bool Triggered => m_Triggered;

	private bool IsOnCooldown
	{
		get
		{
			if (base.Settings.HasCooldown)
			{
				return (m_GameTime - m_TriggeredTime).TotalSeconds < (double)base.Settings.Cooldown;
			}
			return false;
		}
	}

	private TimeSpan m_GameTime => Game.Instance.Controllers.TimeController.GameTime;

	public void Trigger()
	{
		if (CanBeTriggered)
		{
			m_TriggeredTime = m_GameTime;
			m_Triggered = true;
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		Game.Instance.Controllers.ProximityAsksController.Register(this);
	}

	protected override void OnViewWillDetach()
	{
		Game.Instance.Controllers.ProximityAsksController.Unregister(this);
		base.OnViewWillDetach();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Triggered);
		result.Append(ref m_TriggeredTime);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AskPart source = new AskPart();
		result = Unsafe.As<AskPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AskPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "m_Triggered", ref m_Triggered, state);
		formatter.Field(2, "m_TriggeredTime", ref m_TriggeredTime, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AskPart>();
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
				m_Triggered = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_TriggeredTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
