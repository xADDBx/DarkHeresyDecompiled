using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class DialogAnswerGameCommand : GameCommand, IOwlPackable<DialogAnswerGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_Tick;

	[JsonProperty]
	[OwlPackInclude]
	private string m_Answer;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DialogAnswerGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Tick", typeof(int)),
			new FieldInfo("m_Answer", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	private DialogAnswerGameCommand()
	{
	}

	[JsonConstructor]
	public DialogAnswerGameCommand(int tick, [NotNull] string answer)
	{
		m_Tick = tick;
		m_Answer = answer;
	}

	protected override void ExecuteInternal()
	{
		if (!Game.Instance.Controllers.DialogController.CuePlayScheduled && m_Tick >= Game.Instance.Controllers.DialogController.CurrentCueUpdateTick)
		{
			Game.Instance.Controllers.DialogController.SelectAnswer(m_Answer);
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DialogAnswerGameCommand source = new DialogAnswerGameCommand();
		result = Unsafe.As<DialogAnswerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DialogAnswerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Tick", ref m_Tick, state);
		formatter.StringField(1, "m_Answer", ref m_Answer, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DialogAnswerGameCommand>();
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
				m_Tick = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_Answer = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
