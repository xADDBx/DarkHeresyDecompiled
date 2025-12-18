using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Settings;

[OwlPackable(OwlPackableMode.Generate)]
public class GameTurnBasedValues : IOwlPackable, IOwlPackable<GameTurnBasedValues>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "GameTurnBasedValues",
		OldNames = null,
		Fields = new FieldInfo[6]
		{
			new FieldInfo("SpeedUpMode", typeof(SpeedUpMode)),
			new FieldInfo("FastMovement", typeof(bool)),
			new FieldInfo("FastPartyCast", typeof(bool)),
			new FieldInfo("DisableActionCamera", typeof(bool)),
			new FieldInfo("TimeScaleInPlayerTurn", typeof(float)),
			new FieldInfo("TimeScaleInNonPlayerTurn", typeof(float))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public SpeedUpMode SpeedUpMode { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool FastMovement { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool FastPartyCast { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool DisableActionCamera { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float TimeScaleInPlayerTurn { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float TimeScaleInNonPlayerTurn { get; set; }

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		GameTurnBasedValues source = new GameTurnBasedValues();
		result = Unsafe.As<GameTurnBasedValues, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<GameTurnBasedValues>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		SpeedUpMode value = SpeedUpMode;
		formatter.EnumField(0, "SpeedUpMode", ref value, state);
		bool value2 = FastMovement;
		formatter.UnmanagedField(1, "FastMovement", ref value2, state);
		bool value3 = FastPartyCast;
		formatter.UnmanagedField(2, "FastPartyCast", ref value3, state);
		bool value4 = DisableActionCamera;
		formatter.UnmanagedField(3, "DisableActionCamera", ref value4, state);
		float value5 = TimeScaleInPlayerTurn;
		formatter.UnmanagedField(4, "TimeScaleInPlayerTurn", ref value5, state);
		float value6 = TimeScaleInNonPlayerTurn;
		formatter.UnmanagedField(5, "TimeScaleInNonPlayerTurn", ref value6, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<GameTurnBasedValues>();
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
				SpeedUpMode = formatter.ReadEnum<SpeedUpMode>(state);
				break;
			case 1:
				FastMovement = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				FastPartyCast = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				DisableActionCamera = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				TimeScaleInPlayerTurn = formatter.ReadUnmanaged<float>(state);
				break;
			case 5:
				TimeScaleInNonPlayerTurn = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
