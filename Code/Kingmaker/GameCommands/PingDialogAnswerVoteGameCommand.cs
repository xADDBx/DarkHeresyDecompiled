using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PingDialogAnswerVoteGameCommand : GameCommand, IOwlPackable<PingDialogAnswerVoteGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_Answer;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PingDialogAnswerVoteGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Answer", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingDialogAnswerVoteGameCommand()
	{
	}

	public PingDialogAnswerVoteGameCommand(string m_answer)
		: this()
	{
		m_Answer = m_answer;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerVoteLocally(playerOrEmpty, m_Answer);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingDialogAnswerVoteGameCommand source = new PingDialogAnswerVoteGameCommand();
		result = Unsafe.As<PingDialogAnswerVoteGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingDialogAnswerVoteGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Answer", ref m_Answer, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingDialogAnswerVoteGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
