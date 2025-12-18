using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[OwlPackable(OwlPackableMode.Generate)]
public readonly struct ElevatorPlatformPassenger : IOwlPackable, IOwlPackable<ElevatorPlatformPassenger>
{
	[OwlPackInclude]
	private readonly EntityRef _ref;

	[OwlPackInclude]
	public readonly Vector3 InitialPosition;

	[OwlPackInclude]
	public readonly float InitialOrientation;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ElevatorPlatformPassenger",
		Fields = new FieldInfo[3]
		{
			new FieldInfo("_ref", typeof(EntityRef)),
			new FieldInfo("InitialPosition", typeof(Vector3)),
			new FieldInfo("InitialOrientation", typeof(float))
		}
	};

	public MechanicEntity? Entity => _ref.Entity as MechanicEntity;

	public ElevatorPlatformPassenger(MechanicEntity entity)
	{
		_ref = entity.Ref;
		InitialPosition = entity.Position;
		InitialOrientation = entity.Orientation;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ElevatorPlatformPassenger source = default(ElevatorPlatformPassenger);
		result = Unsafe.As<ElevatorPlatformPassenger, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ElevatorPlatformPassenger>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef value = _ref;
		formatter.Field(0, "_ref", ref value, state);
		Vector3 value2 = InitialPosition;
		formatter.Field(1, "InitialPosition", ref value2, state);
		float value3 = InitialOrientation;
		formatter.UnmanagedField(2, "InitialOrientation", ref value3, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ElevatorPlatformPassenger>();
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
				Unsafe.AsRef(in _ref) = formatter.ReadPackable<EntityRef>(state);
				break;
			case 1:
				Unsafe.AsRef(in InitialPosition) = formatter.ReadPackable<Vector3>(state);
				break;
			case 2:
				Unsafe.AsRef(in InitialOrientation) = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
