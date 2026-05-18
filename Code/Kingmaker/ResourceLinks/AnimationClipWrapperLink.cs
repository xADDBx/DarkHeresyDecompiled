using System;
using System.Buffers;
using Kingmaker.Visual.Animation;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class AnimationClipWrapperLink : ScriptableObjectLink<AnimationClipWrapper, AnimationClipWrapperLink>, IMemoryPackable<AnimationClipWrapperLink>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class AnimationClipWrapperLinkFormatter : MemoryPackFormatter<AnimationClipWrapperLink>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AnimationClipWrapperLink value)
		{
			AnimationClipWrapperLink.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AnimationClipWrapperLink value)
		{
			AnimationClipWrapperLink.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AnimationClipWrapperLink value)
		{
			AnimationClipWrapperLink.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AnimationClipWrapperLink value)
		{
			AnimationClipWrapperLink.DeserializeJson(ref reader, ref value);
		}
	}

	static AnimationClipWrapperLink()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AnimationClipWrapperLink>())
		{
			MemoryPackFormatterProvider.Register(new AnimationClipWrapperLinkFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AnimationClipWrapperLink[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AnimationClipWrapperLink>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AnimationClipWrapperLink? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.AssetId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AnimationClipWrapperLink? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string assetId;
		if (memberCount == 1)
		{
			if (!(value == null))
			{
				assetId = value.AssetId;
				assetId = reader.ReadString();
				goto IL_007a;
			}
			assetId = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AnimationClipWrapperLink), 1, memberCount);
				return;
			}
			assetId = ((!(value == null)) ? value.AssetId : null);
			if (memberCount != 0)
			{
				assetId = reader.ReadString();
				_ = 1;
			}
			if (!(value == null))
			{
				goto IL_007a;
			}
		}
		value = new AnimationClipWrapperLink
		{
			AssetId = assetId
		};
		return;
		IL_007a:
		value.AssetId = assetId;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AnimationClipWrapperLink? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("AssetId");
		writer.WriteString(value.AssetId);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AnimationClipWrapperLink? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string assetId = ((!(value == null)) ? value.AssetId : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "AssetId")
				{
					assetId = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text == "AssetId")
			{
				assetId = reader.ReadString();
			}
		}
		if (!(value == null))
		{
			value.AssetId = assetId;
		}
		else
		{
			value = new AnimationClipWrapperLink
			{
				AssetId = assetId
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
