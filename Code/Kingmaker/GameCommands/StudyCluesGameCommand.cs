using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class StudyCluesGameCommand : GameCommand, IMemoryPackable<StudyCluesGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<StudyCluesGameCommand>
{
	[Preserve]
	private sealed class StudyCluesGameCommandFormatter : MemoryPackFormatter<StudyCluesGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StudyCluesGameCommand value)
		{
			StudyCluesGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref StudyCluesGameCommand value)
		{
			StudyCluesGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StudyCluesGameCommand value)
		{
			StudyCluesGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref StudyCluesGameCommand value)
		{
			StudyCluesGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<BlueprintClueStudyReference> _cluesToStudy;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private StudyCluesGameCommand()
	{
	}

	[JsonConstructor]
	public StudyCluesGameCommand(List<BlueprintClueStudyReference> cluesToStudy)
	{
		_cluesToStudy = cluesToStudy;
	}

	protected override void ExecuteInternal()
	{
		DetectiveSystem detectiveSystem = Game.Instance.DetectiveSystem;
		foreach (BlueprintClueStudyReference item in _cluesToStudy)
		{
			detectiveSystem.StudyClue(item);
		}
		EventBus.RaiseEvent(delegate(IClueStudyHandler h)
		{
			h.HandleClueStudied();
		});
	}

	static StudyCluesGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "StudyCluesGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("_cluesToStudy", typeof(List<BlueprintClueStudyReference>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StudyCluesGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StudyCluesGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StudyCluesGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StudyCluesGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<BlueprintClueStudyReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BlueprintClueStudyReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StudyCluesGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		ListFormatter.SerializePackable(ref writer, value._cluesToStudy);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StudyCluesGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<BlueprintClueStudyReference> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value._cluesToStudy;
				ListFormatter.DeserializePackable(ref reader, ref value2);
				goto IL_006a;
			}
			value2 = ListFormatter.DeserializePackable<BlueprintClueStudyReference>(ref reader);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StudyCluesGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value._cluesToStudy : null);
			if (memberCount != 0)
			{
				ListFormatter.DeserializePackable(ref reader, ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new StudyCluesGameCommand
		{
			_cluesToStudy = value2
		};
		return;
		IL_006a:
		value._cluesToStudy = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref StudyCluesGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("_cluesToStudy");
		ListFormatter.SerializePackableJson(ref writer, value._cluesToStudy);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref StudyCluesGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		List<BlueprintClueStudyReference> value2 = ((value != null) ? value._cluesToStudy : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "_cluesToStudy")
				{
					value2 = ListFormatter.DeserializePackableJson<BlueprintClueStudyReference>(ref reader);
					array[0] = true;
				}
			}
			else if (text == "_cluesToStudy")
			{
				ListFormatter.DeserializePackableJson(ref reader, ref value2);
			}
		}
		if (value != null)
		{
			value._cluesToStudy = value2;
		}
		else
		{
			value = new StudyCluesGameCommand
			{
				_cluesToStudy = value2
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StudyCluesGameCommand source = new StudyCluesGameCommand();
		result = Unsafe.As<StudyCluesGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StudyCluesGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_cluesToStudy", ref _cluesToStudy, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StudyCluesGameCommand>();
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
				_cluesToStudy = formatter.ReadPackable<List<BlueprintClueStudyReference>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
