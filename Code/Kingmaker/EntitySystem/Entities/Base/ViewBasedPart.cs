using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[OwlPackable(OwlPackableMode.Generate)]
public class ViewBasedPart : EntityPart<Entity>, IHashable, IOwlPackable<ViewBasedPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ViewBasedPart",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("SourceType", typeof(string))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public string SourceType { get; protected set; }

	public IAbstractEntityPartComponent Source { get; private set; }

	public IEntityViewBase View => base.Owner.View;

	public virtual bool ShouldCheckSourceComponent => true;

	public virtual void SetSource(IAbstractEntityPartComponent source)
	{
		Source = source;
		SourceType = source.GetType().Name;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(SourceType);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ViewBasedPart source = new ViewBasedPart();
		result = Unsafe.As<ViewBasedPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ViewBasedPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ViewBasedPart>();
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
				SourceType = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class ViewBasedPart<TSettings> : ViewBasedPart, IHashable, IOwlPackable<ViewBasedPart<TSettings>> where TSettings : class, new()
{
	public TSettings Settings { get; private set; } = new TSettings();


	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public abstract override void Serialize<TFormatter>(TFormatter formatter, SerializerState state);

	public abstract override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state);
}
