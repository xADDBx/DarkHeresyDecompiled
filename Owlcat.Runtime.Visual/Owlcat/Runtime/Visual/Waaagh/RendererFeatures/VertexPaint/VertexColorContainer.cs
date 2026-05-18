using System;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Math;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public sealed class VertexColorContainer : MonoBehaviour
{
	[Flags]
	private enum ExternalDataFetchFlags
	{
		None = 0,
		CreateIfNotPresent = 1,
		ForModification = 2
	}

	private ref struct UncompressedColor
	{
		public const int kByteCount = 4;

		private const uint kTwoChannelsMask = 15u;

		private const uint kTwoChannelsScale = 16u;

		private const uint kFourChannelsMask = 7u;

		private const uint kFourChannelsScale = 32u;

		public unsafe fixed byte UnpackedValue[4];

		public unsafe void Unpack(byte packedValue, int channelCount)
		{
			switch (channelCount)
			{
			case 1:
				UnpackedValue[0] = packedValue;
				break;
			case 2:
				UnpackedValue[0] = (byte)((int)((long)(int)packedValue & 0xFL) * 16);
				UnpackedValue[1] = (byte)((int)((long)(packedValue >> 4) & 0xFL) * 16);
				break;
			case 3:
			case 4:
				UnpackedValue[0] = (byte)((int)((long)(int)packedValue & 7L) * 32);
				UnpackedValue[1] = (byte)((int)((long)(packedValue >> 2) & 7L) * 32);
				UnpackedValue[2] = (byte)((int)((long)(packedValue >> 4) & 7L) * 32);
				UnpackedValue[3] = (byte)((int)((long)(packedValue >> 6) & 7L) * 32);
				break;
			default:
				throw new ArgumentException("channelCount");
			}
		}

		public unsafe byte Pack(int channelCount)
		{
			switch (channelCount)
			{
			case 1:
				return UnpackedValue[0];
			case 2:
				return (byte)(((uint)UnpackedValue[1] / 16u << 4) | ((uint)UnpackedValue[0] / 16u));
			case 3:
			case 4:
				return (byte)(((uint)UnpackedValue[3] / 32u << 6) | ((uint)UnpackedValue[2] / 32u << 4) | ((uint)UnpackedValue[1] / 32u << 2) | UnpackedValue[0]);
			default:
				throw new ArgumentException("channelCount");
			}
		}
	}

	[Flags]
	public enum ColorChannels
	{
		None = 0,
		R = 1,
		G = 2,
		B = 4,
		A = 8,
		All = 0xF
	}

	public struct RawColorsData
	{
		public byte[] Colors;

		public int SourceInstanceID;
	}

	private const int kBufferStride = 4;

	[SerializeField]
	private ColorChannels m_Channels = ColorChannels.All;

	[SerializeField]
	private bool m_IsCompressed;

	[SerializeField]
	private byte[] m_RawColors = Array.Empty<byte>();

	[SerializeField]
	[CanBeNull]
	[HideInInspector]
	private VertexColorDataAsset m_ExternalAsset;

	[SerializeField]
	[CanBeNull]
	[HideInInspector]
	private VertexColorDataAsset m_InlineAsset;

	[SerializeField]
	[HideInInspector]
	private int m_ColorsCount;

	[SerializeField]
	private Color m_LODFadeColor = Color.clear;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_LODFadeStart = 1f;

	public ColorChannels Channels => m_Channels;

	public bool IsCompressed => m_IsCompressed;

	public int ColorsCount => m_ColorsCount;

	public float LODFadeStart => m_LODFadeStart;

	public Color LODFadeColor => m_LODFadeColor;

	public event Action<VertexColorContainer> ColorsChanged;

	private ref byte[] GetRawColors(ExternalDataFetchFlags fetchFlags)
	{
		VertexColorDataAsset colorDataAssetOrDefault = GetColorDataAssetOrDefault(fetchFlags);
		if (colorDataAssetOrDefault != null)
		{
			return ref colorDataAssetOrDefault.RawColors;
		}
		return ref m_RawColors;
	}

	private int GetRawColorsSourceInstanceID(ExternalDataFetchFlags fetchFlags)
	{
		VertexColorDataAsset colorDataAssetOrDefault = GetColorDataAssetOrDefault(fetchFlags);
		if (colorDataAssetOrDefault != null)
		{
			return colorDataAssetOrDefault.GetInstanceID();
		}
		return GetInstanceID();
	}

	private bool IsPrefab(out string assetPath)
	{
		assetPath = null;
		return false;
	}

	[CanBeNull]
	private VertexColorDataAsset GetColorDataAssetOrDefault(ExternalDataFetchFlags fetchFlags)
	{
		if (m_InlineAsset != null)
		{
			return m_InlineAsset;
		}
		if (m_ExternalAsset != null)
		{
			return m_ExternalAsset;
		}
		return null;
	}

	private void ClearInternalRawColorsIfNotUsed()
	{
		if (m_ExternalAsset != null || m_InlineAsset != null)
		{
			m_RawColors = Array.Empty<byte>();
		}
	}

	private void OnEnable()
	{
		VertexPaintManager.OnEnableContainer(this);
	}

	private void OnDisable()
	{
		VertexPaintManager.OnDisableContainer(this);
	}

	private void OnValidate()
	{
	}

	public RawColorsData GetRawColorsData()
	{
		RawColorsData result = default(RawColorsData);
		result.Colors = GetRawColors(ExternalDataFetchFlags.None);
		result.SourceInstanceID = GetRawColorsSourceInstanceID(ExternalDataFetchFlags.None);
		return result;
	}

	[ContextMenu("Compress")]
	public void Compress()
	{
		NativeArray<Color32> source = new NativeArray<Color32>(m_ColorsCount, Allocator.Temp);
		GetColors(source);
		SetColors(m_Channels, source, compress: true);
	}

	[ContextMenu("Compress", true)]
	private bool CompressValidate()
	{
		return !m_IsCompressed;
	}

	[ContextMenu("Uncompress")]
	public void Uncompress()
	{
		NativeArray<Color32> source = new NativeArray<Color32>(m_ColorsCount, Allocator.Temp);
		GetColors(source);
		SetColors(m_Channels, source, compress: false);
	}

	[ContextMenu("Uncompress", true)]
	private bool UncompressValidate()
	{
		return m_IsCompressed;
	}

	public void DestroyColorDataAssets()
	{
		if (IsPrefab(out var _) && m_ExternalAsset != null)
		{
			UnityEngine.Object.DestroyImmediate(m_ExternalAsset, allowDestroyingAssets: true);
			m_ExternalAsset = null;
		}
		if (m_InlineAsset != null)
		{
			UnityEngine.Object.DestroyImmediate(m_InlineAsset, allowDestroyingAssets: true);
			m_InlineAsset = null;
		}
	}

	public unsafe void SetColors(ColorChannels channels, ReadOnlySpan<Color32> colors, bool compress)
	{
		m_Channels = channels;
		m_ColorsCount = colors.Length;
		m_IsCompressed = compress;
		ref byte[] rawColors = ref GetRawColors(ExternalDataFetchFlags.CreateIfNotPresent | ExternalDataFetchFlags.ForModification);
		if (colors.Length == 0)
		{
			rawColors = Array.Empty<byte>();
		}
		else
		{
			int num = CountChannels(m_Channels);
			int value = (m_IsCompressed ? colors.Length : (num * colors.Length));
			value = Alignment.AlignUp(value, 4);
			if (rawColors == null || rawColors.Length != value)
			{
				rawColors = new byte[value];
			}
			for (int i = 0; i < colors.Length; i++)
			{
				Color32 color = colors[i];
				if (m_IsCompressed)
				{
					UncompressedColor uncompressedColor = default(UncompressedColor);
					Span<byte> bytes = new Span<byte>(uncompressedColor.UnpackedValue, 4);
					int offset = 0;
					WriteChannelConditional(bytes, ref offset, ColorChannels.R, color.r);
					WriteChannelConditional(bytes, ref offset, ColorChannels.G, color.g);
					WriteChannelConditional(bytes, ref offset, ColorChannels.B, color.b);
					WriteChannelConditional(bytes, ref offset, ColorChannels.A, color.a);
					rawColors[i] = uncompressedColor.Pack(num);
				}
				else
				{
					int offset2 = i * num;
					WriteChannelConditional(rawColors, ref offset2, ColorChannels.R, color.r);
					WriteChannelConditional(rawColors, ref offset2, ColorChannels.G, color.g);
					WriteChannelConditional(rawColors, ref offset2, ColorChannels.B, color.b);
					WriteChannelConditional(rawColors, ref offset2, ColorChannels.A, color.a);
				}
			}
		}
		ClearInternalRawColorsIfNotUsed();
		OnColorsChanged();
	}

	public unsafe void GetColors(NativeArray<Color32> colors)
	{
		ref byte[] rawColors = ref GetRawColors(ExternalDataFetchFlags.None);
		int offset = 0;
		int channelCount = CountChannels(m_Channels);
		for (int i = 0; i < m_ColorsCount; i++)
		{
			Color32 value = default(Color32);
			if (m_IsCompressed)
			{
				byte packedValue = rawColors[offset++];
				UncompressedColor uncompressedColor = default(UncompressedColor);
				uncompressedColor.Unpack(packedValue, channelCount);
				Span<byte> bytes = new Span<byte>(uncompressedColor.UnpackedValue, 4);
				int offset2 = 0;
				value.r = ReadChannelConditionalOrDefault(bytes, ref offset2, ColorChannels.R);
				value.g = ReadChannelConditionalOrDefault(bytes, ref offset2, ColorChannels.G);
				value.b = ReadChannelConditionalOrDefault(bytes, ref offset2, ColorChannels.B);
				value.a = ReadChannelConditionalOrDefault(bytes, ref offset2, ColorChannels.A);
			}
			else
			{
				value.r = ReadChannelConditionalOrDefault(rawColors, ref offset, ColorChannels.R);
				value.g = ReadChannelConditionalOrDefault(rawColors, ref offset, ColorChannels.G);
				value.b = ReadChannelConditionalOrDefault(rawColors, ref offset, ColorChannels.B);
				value.a = ReadChannelConditionalOrDefault(rawColors, ref offset, ColorChannels.A);
			}
			colors[i] = value;
		}
	}

	internal void OnColorsChanged()
	{
		this.ColorsChanged?.Invoke(this);
	}

	private void WriteChannelConditional(Span<byte> bytes, ref int offset, ColorChannels mask, byte channelValue)
	{
		if ((m_Channels & mask) != 0)
		{
			bytes[offset++] = channelValue;
		}
	}

	private byte ReadChannelConditionalOrDefault(Span<byte> bytes, ref int offset, ColorChannels mask)
	{
		if ((m_Channels & mask) != 0)
		{
			return bytes[offset++];
		}
		return 0;
	}

	private static int CountChannels(ColorChannels channels)
	{
		int num = 0;
		if ((channels & ColorChannels.R) != 0)
		{
			num++;
		}
		if ((channels & ColorChannels.G) != 0)
		{
			num++;
		}
		if ((channels & ColorChannels.B) != 0)
		{
			num++;
		}
		if ((channels & ColorChannels.A) != 0)
		{
			num++;
		}
		return num;
	}
}
