using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

[OwlPackable(OwlPackableMode.Generate)]
public class DetectiveClueSignalPart : EntityPartWithConfig<DetectiveRadarSignalSettings>, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IHashable, IOwlPackable<DetectiveClueSignalPart>
{
	[JsonProperty]
	[OwlPackInclude]
	public bool IsJammed;

	[JsonProperty]
	[OwlPackInclude]
	public bool Enabled = true;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveClueSignalPart",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsJammed", typeof(bool)),
			new FieldInfo("Enabled", typeof(bool))
		}
	};

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		Game.Instance.Controllers.DetectiveRadarController.RegisterSignalSource(this);
	}

	protected override void OnViewWillDetach()
	{
		Game.Instance.Controllers.DetectiveRadarController.UnregisterSignalSource(this);
		base.OnViewWillDetach();
	}

	public void HandleObjectInGameChanged()
	{
		Game.Instance.Controllers.DetectiveRadarController.SignalInGameChanged(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsJammed);
		result.Append(ref Enabled);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveClueSignalPart source = new DetectiveClueSignalPart();
		result = Unsafe.As<DetectiveClueSignalPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectiveClueSignalPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsJammed", ref IsJammed, state);
		formatter.UnmanagedField(2, "Enabled", ref Enabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveClueSignalPart>();
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
				IsJammed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
