using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenChangeAppearancePageGameCommand : GameCommand, IOwlPackable<CharGenChangeAppearancePageGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly CharGenAppearancePageType m_PageType;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenChangeAppearancePageGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_PageType", typeof(CharGenAppearancePageType))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenChangeAppearancePageGameCommand(CharGenAppearancePageType m_pageType)
	{
		m_PageType = m_pageType;
	}

	private CharGenChangeAppearancePageGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseHandler h)
		{
			h.HandleAppearancePageChange(m_PageType);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangeAppearancePageGameCommand source = new CharGenChangeAppearancePageGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangeAppearancePageGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangeAppearancePageGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenAppearancePageType value = m_PageType;
		formatter.EnumField(0, "m_PageType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangeAppearancePageGameCommand>();
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
				Unsafe.AsRef(in m_PageType) = formatter.ReadEnum<CharGenAppearancePageType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
