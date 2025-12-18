using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.Gameplay.Components;
using Kingmaker.Items.Slots;
using OwlPack.Runtime;

namespace Kingmaker.Gameplay.Features.Items.Utility;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class EmptyHandWeaponEntry : IOwlPackable, IOwlPackable<EmptyHandWeaponEntry>
{
	[OwlPackInclude]
	private readonly EntityFactRef _factRef;

	private AddEmptyHandWeapon? _settings;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EmptyHandWeaponEntry",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("_factRef", typeof(EntityFactRef)),
			new FieldInfo("SettingsRef", typeof(BlueprintComponentReference))
		}
	};

	[OwlPackInclude]
	[UsedImplicitly]
	private BlueprintComponentReference SettingsRef
	{
		get
		{
			return _settings;
		}
		set
		{
			_settings = (AddEmptyHandWeapon)value.Get();
		}
	}

	public bool IsValid => _settings != null;

	public BlueprintItemWeapon Blueprint => _settings?.Weapon.MaybeBlueprint ?? throw new InvalidOperationException();

	private EmptyHandWeaponEntry(OwlPackConstructorParameter _)
	{
	}

	public EmptyHandWeaponEntry(EntityFact fact, AddEmptyHandWeapon settings)
	{
		_factRef = fact;
		_settings = settings;
	}

	public bool IsFrom(EntityFact fact, AddEmptyHandWeapon settings)
	{
		if (_factRef == fact)
		{
			return _settings == settings;
		}
		return false;
	}

	public bool IsSuitableFor(HandSlot slot)
	{
		if (_settings == null)
		{
			throw new InvalidOperationException();
		}
		if (_settings.WeaponSet switch
		{
			AddEmptyHandWeapon.WeaponSetType.Both => 1, 
			AddEmptyHandWeapon.WeaponSetType.First => (slot.HandsEquipmentSetIndex == 0) ? 1 : 0, 
			AddEmptyHandWeapon.WeaponSetType.Second => (slot.HandsEquipmentSetIndex == 1) ? 1 : 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		} == 0)
		{
			return false;
		}
		return _settings.Hand switch
		{
			AddEmptyHandWeapon.HandType.Both => true, 
			AddEmptyHandWeapon.HandType.Primary => slot.IsPrimaryHand, 
			AddEmptyHandWeapon.HandType.Secondary => !slot.IsPrimaryHand, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EmptyHandWeaponEntry source = new EmptyHandWeaponEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<EmptyHandWeaponEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EmptyHandWeaponEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityFactRef value = _factRef;
		formatter.Field(0, "_factRef", ref value, state);
		BlueprintComponentReference value2 = SettingsRef;
		formatter.Field(1, "SettingsRef", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EmptyHandWeaponEntry>();
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
				Unsafe.AsRef(in _factRef) = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 1:
				SettingsRef = formatter.ReadPackable<BlueprintComponentReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
