using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Kingmaker.Settings.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.CircularReference, SerializeLayout.Explicit)]
public sealed class EnumSettingNetData : TypedBaseSettingNetData<int>, IMemoryPackable<EnumSettingNetData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EnumSettingNetDataFormatter : MemoryPackFormatter<EnumSettingNetData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EnumSettingNetData value)
		{
			EnumSettingNetData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EnumSettingNetData value)
		{
			EnumSettingNetData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EnumSettingNetData value)
		{
			EnumSettingNetData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EnumSettingNetData value)
		{
			EnumSettingNetData.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private EnumSettingNetData()
	{
	}

	public EnumSettingNetData(byte index, int value)
		: base(index, value)
	{
	}

	public override void ForceSet()
	{
		((ISettingsEntityEnum)PhotonManager.Settings.SettingsForSync[base.Index]).SetValueAndConfirm(base.Value);
	}

	static EnumSettingNetData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EnumSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new EnumSettingNetDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EnumSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EnumSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EnumSettingNetData? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		var (flag, num) = writer.OptionalState.GetOrAddReference(value);
		if (flag)
		{
			writer.WriteObjectReferenceId(num);
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteVarInt(Unsafe.SizeOf<byte>());
		writer.WriteVarInt(Unsafe.SizeOf<int>());
		writer.WriteVarInt(num);
		byte value2 = value.Index;
		int value3 = value.Value;
		writer.WriteUnmanaged(in value2, in value3);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EnumSettingNetData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		uint id;
		if (memberCount == 250)
		{
			id = reader.ReadVarIntUInt32();
			value = (EnumSettingNetData)reader.OptionalState.GetObjectReference(id);
			return;
		}
		Span<int> span = stackalloc int[(int)memberCount];
		for (int i = 0; i < memberCount; i++)
		{
			span[i] = reader.ReadVarIntInt32();
		}
		id = reader.ReadVarIntUInt32();
		if (value == null)
		{
			value = new EnumSettingNetData();
		}
		reader.OptionalState.AddObjectReference(id, value);
		int num = 2;
		byte value2;
		int value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.Index;
				value3 = value.Value;
				if (span[0] != 0)
				{
					reader.ReadUnmanaged<byte>(out value2);
				}
				if (span[1] != 0)
				{
					reader.ReadUnmanaged<int>(out value3);
				}
				goto IL_014f;
			}
			if (span[0] == 0)
			{
				value2 = 0;
			}
			else
			{
				reader.ReadUnmanaged<byte>(out value2);
			}
			if (span[1] == 0)
			{
				value3 = 0;
			}
			else
			{
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
			}
			else
			{
				value2 = value.Index;
				value3 = value.Value;
			}
			if (memberCount != 0)
			{
				if (span[0] != 0)
				{
					reader.ReadUnmanaged<byte>(out value2);
				}
				if (memberCount != 1)
				{
					if (span[1] != 0)
					{
						reader.ReadUnmanaged<int>(out value3);
					}
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_014f;
			}
		}
		value = new EnumSettingNetData
		{
			Index = value2,
			Value = value3
		};
		goto IL_0178;
		IL_0178:
		if (memberCount != num)
		{
			for (int j = num; j < memberCount; j++)
			{
				reader.Advance(span[j]);
			}
		}
		return;
		IL_014f:
		value.Index = value2;
		value.Value = value3;
		goto IL_0178;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EnumSettingNetData? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		var (flag, num) = writer.OptionalState.GetOrAddReference(value);
		if (flag)
		{
			writer.WriteObjectReferenceId(num);
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteObjectID(num);
		writer.WriteProperty("Index");
		writer.WriteUnmanaged(value.Index);
		writer.WriteProperty("Value");
		writer.WriteUnmanaged(value.Value);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EnumSettingNetData? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		byte v;
		int v2;
		if (value == null)
		{
			v = 0;
			v2 = 0;
		}
		else
		{
			v = value.Index;
			v2 = value.Value;
		}
		bool[] array = new bool[2];
		string text = null;
		text = reader.PeekPropertyName();
		if (text == "$ref")
		{
			reader.Advance();
			uint id = reader.ReadValue<uint>();
			if (!reader.CheckObjectEnd())
			{
				throw new Exception("Expected object end");
			}
			value = (EnumSettingNetData)reader.OptionalState.GetObjectReference(id);
		}
		else
		{
			if (text == "$id")
			{
				reader.Advance();
				uint id2 = reader.ReadValue<uint>();
				if (value == null)
				{
					value = new EnumSettingNetData();
				}
				reader.OptionalState.AddObjectReference(id2, value);
			}
			while ((text = reader.ReadPropertyName()) != null)
			{
				if (value == null)
				{
					if (!(text == "Index"))
					{
						if (text == "Value")
						{
							reader.ReadUnmanaged<int>(out v2);
							array[1] = true;
						}
					}
					else
					{
						reader.ReadUnmanaged<byte>(out v);
						array[0] = true;
					}
				}
				else if (!(text == "Index"))
				{
					if (text == "Value")
					{
						reader.ReadUnmanaged<int>(out v2);
					}
				}
				else
				{
					reader.ReadUnmanaged<byte>(out v);
				}
			}
			if (value != null)
			{
				value.Index = v;
				value.Value = v2;
			}
			else
			{
				value = new EnumSettingNetData
				{
					Index = v,
					Value = v2
				};
			}
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}
}
