using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.Signals;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SignalGameCommand : GameCommand, IOwlPackable<SignalGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly uint Key;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SignalGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Key", typeof(uint))
		}
	};

	public override bool IsSynchronized => true;

	public SignalGameCommand(uint key)
	{
		Key = key;
	}

	[JsonConstructor]
	private SignalGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		SignalService.Instance.Write(Key, playerOrEmpty);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SignalGameCommand source = new SignalGameCommand();
		result = Unsafe.As<SignalGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SignalGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		uint value = Key;
		formatter.UnmanagedField(0, "Key", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SignalGameCommand>();
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
				Unsafe.AsRef(in Key) = formatter.ReadUnmanaged<uint>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
