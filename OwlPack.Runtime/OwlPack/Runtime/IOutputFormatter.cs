using System;

namespace OwlPack.Runtime;

public interface IOutputFormatter
{
	public enum ObjectKind : byte
	{
		Null,
		Ref,
		Object
	}

	public delegate void FieldDelegate<TField>(byte fieldId, string name, ref TField? value, SerializerState state);

	public delegate void ElementDelegate<TField>(ref TField? value, SerializerState state);

	void NullObject();

	void StartObject(byte fieldId, ushort type, string name, string typeName, uint objectId);

	void StartObject(ushort type, string typeName, uint objectId);

	void EndObject();

	void ObjectRef(uint objectId);

	void StartArray(byte fieldId, ushort type, string name, string collectionTypeName, string elementTypeName, uint objectId, int count);

	void StartArray(ushort type, string collectionTypeName, string elementTypeName, uint objectId, int count);

	void EndArray();

	void Field<T>(byte fieldId, string name, ref T? value, SerializerState state);

	void NullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct;

	void EnumField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : Enum;

	void EnumNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct, Enum;

	void UnmanagedField<T>(byte fieldId, string name, ref T value, SerializerState state) where T : unmanaged;

	void UnmanagedNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : unmanaged;

	void StringField(byte fieldId, string name, ref string? value, SerializerState state);

	void ArrayElement<T>(ref T? value, SerializerState state);

	void EnumArrayElement<T>(ref T? value, SerializerState state) where T : Enum;

	void EnumNullableArrayElement<T>(ref T? value, SerializerState state) where T : struct, Enum;

	void UnmanagedArrayElement<T>(ref T value, SerializerState state) where T : unmanaged;

	void UnmanagedNullableArrayElement<T>(ref T? value, SerializerState state) where T : unmanaged;

	void StringArrayElement(ref string? value, SerializerState state);

	void StartTuple(byte fieldId, string name, byte count);

	void StartTuple(byte count);

	void NextTupleElement();

	void EndTuple();
}
