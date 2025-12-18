using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Dialog;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class StartScheduledDialogCommand : GameCommand, IOwlPackable<StartScheduledDialogCommand>
{
	private readonly DialogData m_DialogData;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StartScheduledDialogCommand",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public StartScheduledDialogCommand(DialogData dialogData)
	{
		m_DialogData = dialogData;
	}

	private StartScheduledDialogCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		((IDialogControllerStartScheduledDialogImmediately)Game.Instance.Controllers.DialogController).StartScheduledDialogImmediately(m_DialogData);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StartScheduledDialogCommand source = new StartScheduledDialogCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StartScheduledDialogCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StartScheduledDialogCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StartScheduledDialogCommand>();
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
