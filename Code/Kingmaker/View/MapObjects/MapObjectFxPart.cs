using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class MapObjectFxPart : EntityPartWithConfig<MapObjectFxSettings>, IAreaHandler, ISubscriber, IHashable, IOwlPackable<MapObjectFxPart>
{
	private GameObject m_FxInstance;

	private bool m_IsLoaded;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MapObjectFxPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("FxActive", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool FxActive { get; private set; }

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
			FxActive = base.Settings.StartActive;
		}
	}

	public void SetFxActive(bool active)
	{
		if (FxActive != active)
		{
			if ((bool)m_FxInstance)
			{
				FxHelper.Destroy(m_FxInstance);
				m_FxInstance = null;
			}
			if (active)
			{
				m_FxInstance = FxHelper.SpawnFxOnGameObject(base.Settings.FxPrefab, base.Settings.FxRoot ? base.Settings.FxRoot.gameObject : ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
			FxActive = active;
		}
	}

	public void OnAreaBeginUnloading()
	{
		if ((bool)m_FxInstance)
		{
			FxHelper.Destroy(m_FxInstance);
			m_FxInstance = null;
		}
	}

	public void OnAreaDidLoad()
	{
		if (FxActive)
		{
			m_FxInstance = FxHelper.SpawnFxOnGameObject(base.Settings.FxPrefab, base.Settings.FxRoot ? base.Settings.FxRoot.gameObject : ((EntityViewBase)base.View).Or(null)?.gameObject);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = FxActive;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MapObjectFxPart source = new MapObjectFxPart();
		result = Unsafe.As<MapObjectFxPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MapObjectFxPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = FxActive;
		formatter.UnmanagedField(1, "FxActive", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MapObjectFxPart>();
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
				FxActive = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
