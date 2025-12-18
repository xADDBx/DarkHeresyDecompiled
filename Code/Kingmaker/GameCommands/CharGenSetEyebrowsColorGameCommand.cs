using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetEyebrowsColorGameCommand : GameCommand, IOwlPackable<CharGenSetEyebrowsColorGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetEyebrowsColorGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Index", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenSetEyebrowsColorGameCommand(int m_index)
	{
		m_Index = m_index;
	}

	private CharGenSetEyebrowsColorGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetEyebrowsColor(m_Index);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetEyebrowsColorGameCommand source = new CharGenSetEyebrowsColorGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetEyebrowsColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetEyebrowsColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_Index;
		formatter.UnmanagedField(0, "m_Index", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetEyebrowsColorGameCommand>();
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
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
