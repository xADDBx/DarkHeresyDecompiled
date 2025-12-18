using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class LocalMapMarkerPart : ViewBasedPart<LocalMapMarkerSettings>, ILocalMapMarker, IHashable, IOwlPackable<LocalMapMarkerPart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LocalMapMarkerPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("Hidden", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool Hidden { get; private set; }

	public string NonLocalizedDescription { get; set; }

	public bool IsDisposed => base.Owner == null;

	public override bool ShouldCheckSourceComponent
	{
		get
		{
			if (base.ShouldCheckSourceComponent)
			{
				return !IsRuntimeCreated;
			}
			return false;
		}
	}

	public bool IsRuntimeCreated { get; set; }

	public LocalMapMarkType GetMarkerType()
	{
		return base.Settings.Type;
	}

	public string GetDescription()
	{
		if (base.Settings.DescriptionUnit != null)
		{
			return base.Settings.DescriptionUnit.CharacterName;
		}
		if (base.Settings.Description != null)
		{
			return base.Settings.Description.String;
		}
		return NonLocalizedDescription;
	}

	public Vector3 GetPosition()
	{
		return ((EntityViewBase)base.View).Or(null)?.ViewTransform.position ?? Vector3.zero;
	}

	bool ILocalMapMarker.IsMapObject()
	{
		return base.View is MapObjectView;
	}

	public Entity GetEntity()
	{
		return base.Owner;
	}

	bool ILocalMapMarker.IsVisible()
	{
		if (Hidden)
		{
			return false;
		}
		if (base.Owner.IsRevealed && base.Owner.IsInGame)
		{
			return ((MapObjectEntity)base.Owner).IsAwarenessCheckPassed;
		}
		return false;
	}

	public void SetHidden(bool v)
	{
		Hidden = v;
	}

	protected override void OnAttachOrPostLoad()
	{
		LocalMapModel.Add(this);
	}

	protected override void OnDetach()
	{
		LocalMapModel.Remove(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Hidden;
		result.Append(ref val2);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LocalMapMarkerPart source = new LocalMapMarkerPart();
		result = Unsafe.As<LocalMapMarkerPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LocalMapMarkerPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = Hidden;
		formatter.UnmanagedField(1, "Hidden", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LocalMapMarkerPart>();
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
				Hidden = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
