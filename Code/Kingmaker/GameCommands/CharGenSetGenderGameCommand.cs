using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetGenderGameCommand : GameCommand, IOwlPackable<CharGenSetGenderGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly Gender m_Gender;

	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetGenderGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Gender", typeof(Gender)),
			new FieldInfo("m_Index", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenSetGenderGameCommand(Gender m_gender, int m_index)
	{
		m_Gender = m_gender;
		m_Index = m_index;
	}

	private CharGenSetGenderGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetGender(m_Gender, m_Index);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetGenderGameCommand source = new CharGenSetGenderGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetGenderGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetGenderGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Gender value = m_Gender;
		formatter.EnumField(0, "m_Gender", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetGenderGameCommand>();
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
				Unsafe.AsRef(in m_Gender) = formatter.ReadEnum<Gender>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
