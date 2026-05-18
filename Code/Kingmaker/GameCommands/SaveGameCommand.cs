using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
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
public sealed class SaveGameCommand : GameCommand, IMemoryPackable<SaveGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SaveGameCommand>
{
	[Preserve]
	private sealed class SaveGameCommandFormatter : MemoryPackFormatter<SaveGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveGameCommand value)
		{
			SaveGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveGameCommand value)
		{
			SaveGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveGameCommand value)
		{
			SaveGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveGameCommand value)
		{
			SaveGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	private readonly SaveInfo m_SaveInfo;

	private readonly string m_SaveName;

	private readonly Action m_Callback;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly SaveInfo.SaveType m_Type;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SaveGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SaveGameCommand(SaveInfo.SaveType m_type)
	{
		m_Type = m_type;
	}

	public SaveGameCommand([CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName, Action callback = null)
		: this(saveInfo?.Type ?? SaveInfo.SaveType.Manual)
	{
		m_SaveInfo = saveInfo;
		m_SaveName = saveName;
		m_Callback = callback;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		if (!playerOrEmpty.IsLocal)
		{
			AnotherPlayerSavesGame(playerOrEmpty, m_Type);
			return;
		}
		SaveInfo saveInfo = m_SaveInfo ?? Game.Instance.SaveManager.CreateNewSave(m_SaveName);
		if (!string.IsNullOrEmpty(m_SaveName))
		{
			saveInfo.Name = m_SaveName;
		}
		Game.Instance.SaveGame(saveInfo, m_Callback);
	}

	public static bool PreSaveGame(SaveInfo.SaveType saveType)
	{
		if (!Game.Instance.SaveManager.IsSaveAllowed(saveType))
		{
			return false;
		}
		Game.Instance.SaveManager.PreSave();
		return true;
	}

	private static void AnotherPlayerSavesGame(NetPlayer player, SaveInfo.SaveType saveType)
	{
		if (PreSaveGame(saveType) && NetworkingManager.GetNickName(player, out var nickName))
		{
			PFLog.GameCommands.Log("[SaveGameCommand] Player '" + nickName + "' is saving the game...");
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.GameSavedInProgress);
			});
		}
	}

	static SaveGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SaveGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Type", typeof(SaveInfo.SaveType))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SaveGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfo.SaveType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SaveInfo.SaveType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Type);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		SaveInfo.SaveType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
			}
			else
			{
				value2 = value.m_Type;
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Type : SaveInfo.SaveType.Manual);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SaveGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Type");
		writer.WriteUnmanaged(value.m_Type);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		SaveInfo.SaveType v = ((value != null) ? value.m_Type : SaveInfo.SaveType.Manual);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Type")
				{
					reader.ReadUnmanaged<SaveInfo.SaveType>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Type")
			{
				reader.ReadUnmanaged<SaveInfo.SaveType>(out v);
			}
		}
		_ = value;
		value = new SaveGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveGameCommand source = new SaveGameCommand();
		result = Unsafe.As<SaveGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SaveGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		SaveInfo.SaveType value = m_Type;
		formatter.EnumField(0, "m_Type", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveGameCommand>();
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
				Unsafe.AsRef(in m_Type) = formatter.ReadEnum<SaveInfo.SaveType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
