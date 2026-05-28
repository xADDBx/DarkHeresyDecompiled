using System;
using System.Reflection;

namespace OwlPack.Runtime;

public static class Serializer
{
	public const byte Version = 3;

	[Obsolete]
	public static TArchive Serialize<TArchive, T>(ref T? value) where TArchive : IOutputArchive, new() where T : IOwlPackable, IOwlPackable<T>
	{
		TArchive result = new TArchive();
		result.Serialize(ref value);
		return result;
	}

	[Obsolete]
	public static TArchive SerializeAny<TArchive, T>(ref T? value) where TArchive : IOutputArchive, new()
	{
		TArchive result = new TArchive();
		result.SerializeAny(ref value);
		return result;
	}

	[Obsolete]
	public static T? Deserialize<TArchive, T>(TArchive archive) where TArchive : IInputArchive
	{
		return archive.Deserialize<T>();
	}

	public static TPossiblyBase? DeserializeObject<TPossiblyBase>(IInputFormatter formatter, DeserializerState state)
	{
		uint objectId;
		ushort type;
		switch (formatter.ReadObjectHeader(out objectId, out type))
		{
		case IOutputFormatter.ObjectKind.Null:
			return default(TPossiblyBase);
		case IOutputFormatter.ObjectKind.Ref:
			return state.References.Resolve<TPossiblyBase>(objectId);
		default:
		{
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo(type);
			Type typeByID = state.TypeLibrary.GetTypeByID(type);
			Type typeFromHandle = typeof(TPossiblyBase);
			if (typeByID != null && typeByID != typeof(Array) && !typeFromHandle.IsAssignableFrom(typeByID))
			{
				ITypeConverter<TPossiblyBase> converter = TypeConverterLibrary.Instance.GetConverter<TPossiblyBase>(typeByID);
				if (converter == null)
				{
					throw new Exception("Incompatible types: currentType=" + typeFromHandle.FullName + ", serializedType=" + typeByID.FullName);
				}
				object serializedObject = typeof(Serializer).GetMethod("DeserializeObjectBody", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeByID).Invoke(null, new object[5]
				{
					formatter,
					state,
					objectId,
					type,
					state.TypeLibrary.GetTypeInfo(typeByID)
				});
				return converter.Convert(serializedObject);
			}
			return DeserializeObjectBody<TPossiblyBase>(formatter, state, objectId, type, typeInfo);
		}
		}
	}

	private static TPossiblyBase? DeserializeObjectBody<TPossiblyBase>(IInputFormatter formatter, DeserializerState state, uint objectId, ushort typeID, TypeInfo typeInfo)
	{
		if (!typeInfo.IsExternal)
		{
			TPossiblyBase result = default(TPossiblyBase);
			state.TypeLibrary.CreateObject(typeID, ref result);
			if (result is IOwlPackable owlPackable)
			{
				owlPackable.Deserialize(formatter, objectId, state);
				return (TPossiblyBase)owlPackable;
			}
			throw new Exception($"Type {typeInfo.Name} ID={typeID} is no longer OwlPackable, cannot be deserialized");
		}
		TPossiblyBase value = default(TPossiblyBase);
		state.TypeLibrary.GetExternalTypeSerializer(typeID).Deserialize(formatter, ref value, objectId, state);
		return value;
	}

	public static TPossiblyBase? DeserializeNullableStruct<TPossiblyBase>(IInputFormatter formatter, DeserializerState state) where TPossiblyBase : struct
	{
		uint objectId;
		ushort type;
		switch (formatter.ReadObjectHeader(out objectId, out type))
		{
		case IOutputFormatter.ObjectKind.Null:
			return null;
		case IOutputFormatter.ObjectKind.Ref:
			return state.References.Resolve<TPossiblyBase>(objectId);
		default:
		{
			TPossiblyBase value = default(TPossiblyBase);
			if ((object)value is IOwlPackable owlPackable)
			{
				owlPackable.Deserialize(formatter, objectId, state);
				value = (TPossiblyBase)owlPackable;
			}
			else
			{
				state.TypeLibrary.GetExternalTypeSerializer(type).Deserialize(formatter, ref value, objectId, state);
			}
			return value;
		}
		}
	}
}
