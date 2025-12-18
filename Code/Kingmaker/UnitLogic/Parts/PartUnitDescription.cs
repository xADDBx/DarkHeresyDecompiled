using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitDescription : BaseUnitPart, IHashable, IOwlPackable<PartUnitDescription>
{
	public interface IOwner : IEntityPartOwner<PartUnitDescription>, IEntityPartOwner
	{
		PartUnitDescription Description { get; }
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitDescription",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("CustomGender", typeof(Gender?)),
			new FieldInfo("CustomName", typeof(string))
		}
	};

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public Gender? CustomGender { get; private set; }

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public string CustomName { get; private set; }

	public Gender Gender => CustomGender ?? base.Owner.Blueprint.Gender;

	[NotNull]
	public string Name => base.Owner.GetOptional<PartPolymorphed>()?.ReplaceBlueprintForInspection?.CharacterName ?? CustomName ?? base.Owner.Blueprint.CharacterName ?? "";

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			CustomGender = current.Gender;
		}
	}

	public void SetGender(Gender gender)
	{
		CustomGender = gender;
	}

	public void SetName([CanBeNull] string name)
	{
		CustomName = (name.IsNullOrEmpty() ? null : name);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (CustomGender.HasValue)
		{
			Gender val2 = CustomGender.Value;
			result.Append(ref val2);
		}
		result.Append(CustomName);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitDescription source = new PartUnitDescription();
		result = Unsafe.As<PartUnitDescription, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitDescription>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Gender? value = CustomGender;
		formatter.EnumNullableField(0, "CustomGender", ref value, state);
		string value2 = CustomName;
		formatter.StringField(1, "CustomName", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitDescription>();
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
				CustomGender = formatter.ReadNullableEnum<Gender>(state);
				break;
			case 1:
				CustomName = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
