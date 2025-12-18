using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public struct UnitCommandMessage : IOwlPackable, IOwlPackable<UnitCommandMessage>
{
	[JsonProperty(PropertyName = "i")]
	public int tickIndex;

	[JsonProperty(PropertyName = "g")]
	public List<GameCommand> gameCommandList;

	[JsonProperty(PropertyName = "c")]
	public List<UnitCommandParams> unitCommandList;

	[JsonProperty(PropertyName = "d")]
	public List<SynchronizedData> synchronizedDataList;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitCommandMessage",
		Fields = new FieldInfo[0]
	};

	public UnitCommandMessage(int tickIndex, List<GameCommand> gameCommandList, List<UnitCommandParams> unitCommandList, List<SynchronizedData> synchronizedDataList)
	{
		this.tickIndex = tickIndex;
		this.gameCommandList = ((0 < gameCommandList?.Count) ? gameCommandList : null);
		this.unitCommandList = ((0 < unitCommandList?.Count) ? unitCommandList : null);
		this.synchronizedDataList = ((0 < synchronizedDataList?.Count) ? synchronizedDataList : null);
	}

	public void AfterDeserialization()
	{
		if (gameCommandList != null)
		{
			foreach (GameCommand gameCommand in gameCommandList)
			{
				gameCommand.AfterDeserialization();
			}
		}
		if (unitCommandList == null)
		{
			return;
		}
		foreach (UnitCommandParams unitCommand in unitCommandList)
		{
			unitCommand.AfterDeserialization();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitCommandMessage source = default(UnitCommandMessage);
		result = Unsafe.As<UnitCommandMessage, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitCommandMessage>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitCommandMessage>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
