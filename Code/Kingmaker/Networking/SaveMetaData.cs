using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Settings;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public class SaveMetaData : IOwlPackable, IOwlPackable<SaveMetaData>
{
	public static int MaxPacketSize = 49152;

	[JsonProperty(PropertyName = "l")]
	[OwlPackInclude]
	public int length;

	[JsonProperty(PropertyName = "n")]
	[OwlPackInclude]
	public string saveName;

	[JsonProperty(PropertyName = "i")]
	[OwlPackInclude]
	public string saveId;

	[JsonProperty(PropertyName = "r")]
	[OwlPackInclude]
	public uint randomNoise;

	[JsonProperty(PropertyName = "a")]
	[OwlPackInclude]
	public PhotonActorNumber[] actorNumbersAtStart;

	[JsonProperty(PropertyName = "d")]
	[OwlPackInclude]
	public string[] dlcs;

	[JsonProperty(PropertyName = "s")]
	[OwlPackInclude]
	public BaseSettingNetData[] settings;

	[JsonProperty(PropertyName = "p")]
	[OwlPackInclude]
	public PortraitSaveMetaData[] portraitsSaveMeta;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveMetaData",
		OldNames = null,
		Fields = new FieldInfo[8]
		{
			new FieldInfo("length", typeof(int)),
			new FieldInfo("saveName", typeof(string)),
			new FieldInfo("saveId", typeof(string)),
			new FieldInfo("randomNoise", typeof(uint)),
			new FieldInfo("actorNumbersAtStart", typeof(PhotonActorNumber[])),
			new FieldInfo("dlcs", typeof(string[])),
			new FieldInfo("settings", typeof(BaseSettingNetData[])),
			new FieldInfo("portraitsSaveMeta", typeof(PortraitSaveMetaData[]))
		}
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveMetaData source = new SaveMetaData();
		result = Unsafe.As<SaveMetaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SaveMetaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "length", ref length, state);
		formatter.StringField(1, "saveName", ref saveName, state);
		formatter.StringField(2, "saveId", ref saveId, state);
		formatter.UnmanagedField(3, "randomNoise", ref randomNoise, state);
		formatter.Field(4, "actorNumbersAtStart", ref actorNumbersAtStart, state);
		formatter.Field(5, "dlcs", ref dlcs, state);
		formatter.Field(6, "settings", ref settings, state);
		formatter.Field(7, "portraitsSaveMeta", ref portraitsSaveMeta, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveMetaData>();
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
				length = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				saveName = formatter.ReadString(state);
				break;
			case 2:
				saveId = formatter.ReadString(state);
				break;
			case 3:
				randomNoise = formatter.ReadUnmanaged<uint>(state);
				break;
			case 4:
				actorNumbersAtStart = formatter.ReadPackable<PhotonActorNumber[]>(state);
				break;
			case 5:
				dlcs = formatter.ReadPackable<string[]>(state);
				break;
			case 6:
				settings = formatter.ReadPackable<BaseSettingNetData[]>(state);
				break;
			case 7:
				portraitsSaveMeta = formatter.ReadPackable<PortraitSaveMetaData[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
