using System;

namespace OwlPack.Runtime;

public interface IInputFormatter
{
	public delegate TField FieldDelegate<TField>(DeserializerState state);

	public delegate TElement ElementDelegate<TElement>(DeserializerState state);

	IOutputFormatter.ObjectKind ReadObjectHeader(out uint objectId, out ushort type);

	void EnterObject();

	void LeaveObject();

	void ReadFieldHeader(TypeInfo serializedTypeInfo, out byte fieldID, out int size);

	void EnterArray(out int count);

	bool NextArrayElement();

	void LeaveArray();

	void EnterTuple(out byte count);

	bool NextTupleElement();

	void LeaveTuple();

	T ReadUnmanaged<T>(DeserializerState state) where T : unmanaged;

	T? ReadNullableUnmanaged<T>(DeserializerState state) where T : unmanaged;

	string? ReadString(DeserializerState state);

	T ReadEnum<T>(DeserializerState state) where T : Enum;

	T? ReadNullableEnum<T>(DeserializerState state) where T : struct, Enum;

	T ReadPackable<T>(DeserializerState state);

	T? ReadNullablePackable<T>(DeserializerState state) where T : struct;

	T ReadUnmanagedArrayElement<T>(DeserializerState state) where T : unmanaged;

	T? ReadNullableUnmanagedArrayElement<T>(DeserializerState state) where T : unmanaged;

	string? ReadStringArrayElement(DeserializerState state);

	T ReadEnumArrayElement<T>(DeserializerState state) where T : Enum;

	T? ReadNullableEnumArrayElement<T>(DeserializerState state) where T : struct, Enum;

	T ReadPackableArrayElement<T>(DeserializerState state);

	T? ReadNullablePackableArrayElement<T>(DeserializerState state) where T : struct;

	void SkipField(int size);
}
