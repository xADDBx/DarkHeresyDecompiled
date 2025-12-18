using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PingDialogAnswerGameCommand : GameCommand, IOwlPackable<PingDialogAnswerGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Answer;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsHover;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PingDialogAnswerGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Answer", typeof(string)),
			new FieldInfo("m_IsHover", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingDialogAnswerGameCommand()
	{
	}

	public PingDialogAnswerGameCommand(string m_answer, bool m_ishover)
		: this()
	{
		m_Answer = m_answer;
		m_IsHover = m_ishover;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerLocally(playerOrEmpty, m_Answer, m_IsHover);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingDialogAnswerGameCommand source = new PingDialogAnswerGameCommand();
		result = Unsafe.As<PingDialogAnswerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingDialogAnswerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Answer", ref m_Answer, state);
		formatter.UnmanagedField(1, "m_IsHover", ref m_IsHover, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingDialogAnswerGameCommand>();
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
				m_Answer = formatter.ReadString(state);
				break;
			case 1:
				m_IsHover = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
