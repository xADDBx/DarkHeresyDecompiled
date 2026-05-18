using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CloseCaseGameCommand : GameCommand, IMemoryPackable<CloseCaseGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CloseCaseGameCommand>
{
	[Preserve]
	private sealed class CloseCaseGameCommandFormatter : MemoryPackFormatter<CloseCaseGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CloseCaseGameCommand value)
		{
			CloseCaseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CloseCaseGameCommand value)
		{
			CloseCaseGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloseCaseGameCommand value)
		{
			CloseCaseGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CloseCaseGameCommand value)
		{
			CloseCaseGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintCaseReference _case;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintCaseAnswerReference _answer;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public CloseCaseGameCommand()
	{
	}

	[JsonConstructor]
	public CloseCaseGameCommand(BlueprintCaseReference @case, BlueprintCaseAnswerReference answer)
	{
		_case = @case;
		_answer = answer;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.DetectiveSystem.CloseCase(_case, _answer);
	}

	static CloseCaseGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CloseCaseGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("_case", typeof(BlueprintCaseReference)),
				new FieldInfo("_answer", typeof(BlueprintCaseAnswerReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloseCaseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CloseCaseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloseCaseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CloseCaseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CloseCaseGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value._case);
		writer.WritePackable(in value._answer);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CloseCaseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintCaseReference value2;
		BlueprintCaseAnswerReference value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value._case;
				value3 = value._answer;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<BlueprintCaseReference>();
			value3 = reader.ReadPackable<BlueprintCaseAnswerReference>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloseCaseGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value._case;
				value3 = value._answer;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new CloseCaseGameCommand
		{
			_case = value2,
			_answer = value3
		};
		return;
		IL_009a:
		value._case = value2;
		value._answer = value3;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CloseCaseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("_case");
		writer.WritePackable(value._case);
		writer.WriteProperty("_answer");
		writer.WritePackable(value._answer);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CloseCaseGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintCaseReference val;
		BlueprintCaseAnswerReference val2;
		if (value == null)
		{
			val = null;
			val2 = null;
		}
		else
		{
			val = value._case;
			val2 = value._answer;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "_case"))
				{
					if (text == "_answer")
					{
						val2 = reader.ReadPackable<BlueprintCaseAnswerReference>();
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<BlueprintCaseReference>();
					array[0] = true;
				}
			}
			else if (!(text == "_case"))
			{
				if (text == "_answer")
				{
					reader.ReadPackable(ref val2);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value._case = val;
			value._answer = val2;
		}
		else
		{
			value = new CloseCaseGameCommand
			{
				_case = val,
				_answer = val2
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
		CloseCaseGameCommand source = new CloseCaseGameCommand();
		result = Unsafe.As<CloseCaseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CloseCaseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_case", ref _case, state);
		formatter.Field(1, "_answer", ref _answer, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CloseCaseGameCommand>();
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
				_case = formatter.ReadPackable<BlueprintCaseReference>(state);
				break;
			case 1:
				_answer = formatter.ReadPackable<BlueprintCaseAnswerReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
