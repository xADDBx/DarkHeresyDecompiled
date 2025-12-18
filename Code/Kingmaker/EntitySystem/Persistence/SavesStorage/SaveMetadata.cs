using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameInfo;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[OwlPackable(OwlPackableMode.Generate)]
public class SaveMetadata : IOwlPackable, IOwlPackable<SaveMetadata>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveMetadata",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[CanBeNull]
	[JsonProperty]
	public string Username { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string DeviceId { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string CharacterName { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string SaveType { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string SaveName { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Description { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Version { get; set; }

	[JsonProperty]
	public TimeSpan RealTime { get; set; }

	[JsonProperty]
	public TimeSpan GameTime { get; set; }

	[JsonProperty]
	public string GameTimeText { get; set; }

	[JsonProperty]
	public int KingdomDay { get; set; } = -1;


	[CanBeNull]
	[JsonProperty]
	public string Zone { get; set; }

	[JsonProperty]
	public int Level { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Chapter { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string MainQuest { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Alignment { get; set; }

	public SaveMetadata()
	{
	}

	public SaveMetadata([NotNull] SaveInfo save, [CanBeNull] Player player = null)
	{
		Version = GameVersion.Revision;
		Username = Environment.UserName;
		DeviceId = GameVersion.DeviceUniqueIdentifier;
		SaveType = save.Type.ToString();
		SaveName = save.Name;
		CharacterName = save.PlayerCharacterName;
		RealTime = save.GameTotalTime;
		GameTime = save.GameSaveTime;
		GameTimeText = save.GameSaveTimeText;
		if (save.Area != null)
		{
			Zone = save.Area.name;
		}
		if (player == null)
		{
			try
			{
				string source = save.Saver.ReadJson("player");
				player = SaveSystemJsonSerializer.Serializer.DeserializeObject<Player>(source);
			}
			catch (Exception)
			{
				player = null;
			}
		}
		if (player != null)
		{
			if (player.Chapter > 0)
			{
				Chapter = "c" + player.Chapter;
			}
			BaseUnitEntity baseUnitEntity = player.CrossSceneState.AllEntityData.OfType<BaseUnitEntity>().FirstOrDefault((BaseUnitEntity u) => u.UniqueId == player.MainCharacter.Id);
			if (baseUnitEntity != null)
			{
				Level = baseUnitEntity.Progression.CharacterLevel;
			}
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveMetadata source = new SaveMetadata();
		result = Unsafe.As<SaveMetadata, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SaveMetadata>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveMetadata>();
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
