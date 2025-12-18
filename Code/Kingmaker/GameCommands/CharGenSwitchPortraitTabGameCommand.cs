using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSwitchPortraitTabGameCommand : GameCommand, IOwlPackable<CharGenSwitchPortraitTabGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly CharGenPortraitTab m_Tab;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSwitchPortraitTabGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Tab", typeof(CharGenPortraitTab))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenSwitchPortraitTabGameCommand(CharGenPortraitTab m_tab)
	{
		m_Tab = m_tab;
	}

	private CharGenSwitchPortraitTabGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhasePortraitHandler h)
		{
			h.HandlePortraitTabChange(m_Tab);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSwitchPortraitTabGameCommand source = new CharGenSwitchPortraitTabGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSwitchPortraitTabGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSwitchPortraitTabGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenPortraitTab value = m_Tab;
		formatter.EnumField(0, "m_Tab", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSwitchPortraitTabGameCommand>();
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
				Unsafe.AsRef(in m_Tab) = formatter.ReadEnum<CharGenPortraitTab>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
