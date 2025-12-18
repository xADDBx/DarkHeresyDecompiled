using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenShowClothGameCommand : GameCommand, IOwlPackable<CharGenShowClothGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly bool m_ShowCloth;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenShowClothGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_ShowCloth", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenShowClothGameCommand(bool m_showCloth)
	{
		m_ShowCloth = m_showCloth;
	}

	private CharGenShowClothGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
		EventBus.RaiseEvent(delegate(ICharGenVisualHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenShowClothGameCommand source = new CharGenShowClothGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenShowClothGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenShowClothGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = m_ShowCloth;
		formatter.UnmanagedField(0, "m_ShowCloth", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenShowClothGameCommand>();
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
				Unsafe.AsRef(in m_ShowCloth) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
