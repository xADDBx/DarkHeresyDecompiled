using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SaveGameCommand : GameCommand, IOwlPackable<SaveGameCommand>
{
	private readonly SaveInfo m_SaveInfo;

	private readonly string m_SaveName;

	private readonly Action m_Callback;

	[JsonProperty]
	[OwlPackInclude]
	private readonly SaveInfo.SaveType m_Type;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Type", typeof(SaveInfo.SaveType))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SaveGameCommand()
	{
	}

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
