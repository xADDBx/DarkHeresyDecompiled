using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class MusicPlayerPart : EntityPartWithConfig<MusicPlayerSettings>, IAreaHandler, ISubscriber, IHashable, IOwlPackable<MusicPlayerPart>
{
	private bool m_IsLoaded;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MusicPlayerPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsPlaying", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool IsPlaying { get; private set; }

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_IsLoaded = true;
	}

	protected override void OnConfigDidSet(bool isNewConfig)
	{
		base.OnConfigDidSet(isNewConfig);
		if (!m_IsLoaded)
		{
			IsPlaying = base.Settings.AutoPlay;
		}
	}

	public void OnAreaBeginUnloading()
	{
		if (IsPlaying)
		{
			SetPlayingInternal(isPlaying: false);
		}
	}

	public void OnAreaDidLoad()
	{
		if (IsPlaying)
		{
			SetPlayingInternal(isPlaying: true);
		}
	}

	public void SetPlaying(bool isPlaying)
	{
		if (isPlaying != IsPlaying)
		{
			IsPlaying = isPlaying;
			SetPlayingInternal(isPlaying);
		}
	}

	private void SetPlayingInternal(bool isPlaying)
	{
		if (isPlaying)
		{
			string.IsNullOrEmpty(base.Settings.Start);
			string[] startEvents = base.Settings.StartEvents;
			for (int i = 0; i < startEvents.Length; i++)
			{
				SoundEventsManager.PostEvent(startEvents[i], ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
		}
		else
		{
			string.IsNullOrEmpty(base.Settings.Start);
			string[] startEvents = base.Settings.StopEvents;
			for (int i = 0; i < startEvents.Length; i++)
			{
				SoundEventsManager.PostEvent(startEvents[i], ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsPlaying;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MusicPlayerPart source = new MusicPlayerPart();
		result = Unsafe.As<MusicPlayerPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MusicPlayerPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = IsPlaying;
		formatter.UnmanagedField(1, "IsPlaying", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MusicPlayerPart>();
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
				IsPlaying = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
