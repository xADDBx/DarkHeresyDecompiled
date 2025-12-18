using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class FogOfWarRevealerTriggerGameCommand : GameCommand, IOwlPackable<FogOfWarRevealerTriggerGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly string Id;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FogOfWarRevealerTriggerGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Id", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public FogOfWarRevealerTriggerGameCommand(string id)
	{
		Id = id;
	}

	private FogOfWarRevealerTriggerGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		if (!FogOfWarRevealerTrigger.AllTriggers.TryGetValue(Id, out var value))
		{
			PFLog.GameCommands.Error("FogOfWarRevealerTrigger #" + Id + " was not found!");
		}
		else
		{
			value.Reveal();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FogOfWarRevealerTriggerGameCommand source = new FogOfWarRevealerTriggerGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<FogOfWarRevealerTriggerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FogOfWarRevealerTriggerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FogOfWarRevealerTriggerGameCommand>();
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
				Unsafe.AsRef(in Id) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
