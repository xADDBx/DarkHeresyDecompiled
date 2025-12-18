using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class PostSaveCallbackCommand : GameCommand, IOwlPackable<PostSaveCallbackCommand>
{
	public readonly SaveInfo SaveInfo;

	public readonly SaveCreateDTO DTO;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PostSaveCallbackCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public PostSaveCallbackCommand(SaveInfo saveInfo, SaveCreateDTO dto)
	{
		SaveInfo = saveInfo;
		DTO = dto;
	}

	private PostSaveCallbackCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		((ISaveManagerPostSaveCallback)Game.Instance.SaveManager).PostSaveCallback(SaveInfo, DTO);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PostSaveCallbackCommand source = new PostSaveCallbackCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<PostSaveCallbackCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PostSaveCallbackCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PostSaveCallbackCommand>();
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
