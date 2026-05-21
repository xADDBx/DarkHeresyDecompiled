using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public static class VTDiagnostics
{
	internal struct StaleTagInfo
	{
		public int MaterialInstanceId;

		public string MaterialResolved;

		public string ShaderName;

		public int LocalStackIndex;

		public int EntryIndex;

		public int Slot;

		public Guid Guid;
	}

	internal sealed class StaleMissingAnalysis
	{
		public readonly List<StaleTagInfo> Stale = new List<StaleTagInfo>();

		public readonly List<StaleTagInfo> Missing = new List<StaleTagInfo>();

		public int MaterialsWithNoShaderInfo;

		public int StackPairsWithExtras;

		public int StackPairsWithMissing;

		public readonly Dictionary<string, int> StalePerShader = new Dictionary<string, int>();

		public readonly Dictionary<string, int> MissingPerShader = new Dictionary<string, int>();
	}

	private sealed class JsonOut
	{
		private readonly StringBuilder _sb = new StringBuilder(8192);

		private int _indent;

		private bool _needComma;

		public override string ToString()
		{
			return _sb.ToString();
		}

		public void StartObject()
		{
			Sep();
			_sb.Append('{');
			_indent++;
			_needComma = false;
		}

		public void EndObject()
		{
			_indent--;
			NewLine();
			_sb.Append('}');
			_needComma = true;
		}

		public void StartArray()
		{
			Sep();
			_sb.Append('[');
			_indent++;
			_needComma = false;
		}

		public void EndArray()
		{
			_indent--;
			NewLine();
			_sb.Append(']');
			_needComma = true;
		}

		public void PropArray(string name)
		{
			Sep();
			AppendString(name);
			_sb.Append(": [");
			_indent++;
			_needComma = false;
		}

		public void Prop(string name, string v)
		{
			Sep();
			AppendString(name);
			_sb.Append(": ");
			AppendString(v);
			_needComma = true;
		}

		public void Prop(string name, int v)
		{
			Sep();
			AppendString(name);
			_sb.Append(": ").Append(v);
			_needComma = true;
		}

		public void Prop(string name, float v)
		{
			Sep();
			AppendString(name);
			_sb.Append(": ").Append(v.ToString(CultureInfo.InvariantCulture));
			_needComma = true;
		}

		public void Prop(string name, bool v)
		{
			Sep();
			AppendString(name);
			_sb.Append(": ").Append(v ? "true" : "false");
			_needComma = true;
		}

		public void Value(int v)
		{
			Sep();
			_sb.Append(v);
			_needComma = true;
		}

		public void Value(float v)
		{
			Sep();
			_sb.Append(v.ToString(CultureInfo.InvariantCulture));
			_needComma = true;
		}

		private void Sep()
		{
			if (_needComma)
			{
				_sb.Append(',');
			}
			NewLine();
		}

		private void NewLine()
		{
			_sb.Append('\n');
			for (int i = 0; i < _indent; i++)
			{
				_sb.Append("  ");
			}
		}

		private void AppendString(string s)
		{
			_sb.Append('"');
			if (s == null)
			{
				_sb.Append('"');
				return;
			}
			foreach (char c in s)
			{
				switch (c)
				{
				case '\\':
					_sb.Append("\\\\");
					continue;
				case '"':
					_sb.Append("\\\"");
					continue;
				case '\n':
					_sb.Append("\\n");
					continue;
				case '\r':
					_sb.Append("\\r");
					continue;
				case '\t':
					_sb.Append("\\t");
					continue;
				}
				if (c < ' ')
				{
					StringBuilder stringBuilder = _sb.Append("\\u");
					int num = c;
					stringBuilder.Append(num.ToString("X4"));
				}
				else
				{
					_sb.Append(c);
				}
			}
			_sb.Append('"');
		}
	}

	private const float kMaxMipBiasNormalize = 16383f;

	private const int kInt16Max = 32767;

	private const int kMaxInlineErrorLines = 50;

	public static VTDiagnosticsSnapshot Capture(VirtualTextureManager vtm)
	{
		if (vtm == null)
		{
			throw new ArgumentNullException("vtm");
		}
		VTDiagnosticsSnapshot vTDiagnosticsSnapshot = new VTDiagnosticsSnapshot
		{
			CapturedAtUtc = DateTime.UtcNow,
			FrameId = Time.frameCount,
			AtlasResolutionInTiles = vtm.VirtualAtlasResolutionInTiles,
			Occupancy = vtm.VirtualAtlasOccupancy,
			MaxMipCount = vtm.VirtualAtlasMipCount,
			PhysicalAtlasTilesInSliceX = vtm.PhysicalAtlasResolution.TilesInSlice.x,
			PhysicalAtlasTilesInSliceY = vtm.PhysicalAtlasResolution.TilesInSlice.y,
			PhysicalAtlasSliceCount = vtm.PhysicalAtlasResolution.ArraySlices
		};
		VirtualAtlas virtualAtlas = vtm.VirtualAtlas;
		if (virtualAtlas == null)
		{
			return vTDiagnosticsSnapshot;
		}
		vTDiagnosticsSnapshot.AllocatorWidth = virtualAtlas.AtlasAllocator.Width;
		vTDiagnosticsSnapshot.AllocatorHeight = virtualAtlas.AtlasAllocator.Height;
		CaptureEntries(virtualAtlas, vTDiagnosticsSnapshot);
		CaptureMaterials(virtualAtlas, vTDiagnosticsSnapshot);
		CapturePages(virtualAtlas, vTDiagnosticsSnapshot, BuildEntryMipCountMap(vTDiagnosticsSnapshot));
		CapturePhysicalToVirtual(virtualAtlas, vTDiagnosticsSnapshot);
		CaptureGpuBuffer(virtualAtlas, vTDiagnosticsSnapshot);
		CaptureIndirection(vtm, vTDiagnosticsSnapshot);
		CaptureIndirectionDrawData(vtm, vTDiagnosticsSnapshot);
		CapturePaddingLoadRequestEvents(vtm, vTDiagnosticsSnapshot);
		return vTDiagnosticsSnapshot;
	}

	private static void CapturePaddingLoadRequestEvents(VirtualTextureManager vtm, VTDiagnosticsSnapshot snap)
	{
	}

	private static Dictionary<int, int> BuildEntryMipCountMap(VTDiagnosticsSnapshot snap)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(snap.Entries.Count);
		foreach (EntryRecord entry in snap.Entries)
		{
			dictionary[entry.IndexInAllocator] = entry.MipCount;
		}
		return dictionary;
	}

	public static void Write(VTDiagnosticsSnapshot snapshot, string outputDir, IVTAssetInfoResolver resolver = null)
	{
		if (snapshot == null)
		{
			throw new ArgumentNullException("snapshot");
		}
		if (resolver == null)
		{
			resolver = NullVTAssetInfoResolver.Instance;
		}
		Directory.CreateDirectory(outputDir);
		StaleMissingAnalysis analysis = AnalyzeShaderVsMaterial(snapshot, resolver);
		WriteEntriesJson(snapshot, Path.Combine(outputDir, "entries.json"), resolver);
		WriteMaterialsJson(snapshot, Path.Combine(outputDir, "materials.json"), resolver);
		WritePagesJson(snapshot, Path.Combine(outputDir, "pages.json"));
		WriteGpuBufferJson(snapshot, Path.Combine(outputDir, "gpu_buffer.json"));
		WritePhysicalToVirtualJson(snapshot, Path.Combine(outputDir, "physical_to_virtual.json"));
		WriteIndirectionPng(snapshot, Path.Combine(outputDir, "indirection.png"));
		WriteIndirectionDrawDataJson(snapshot, Path.Combine(outputDir, "indirection_draw_data.json"));
		WriteStaleTagsList(analysis, Path.Combine(outputDir, "stale_tags.txt"), resolver);
		WriteMissingTagsList(analysis, Path.Combine(outputDir, "missing_tags.txt"), resolver);
		WriteSummary(snapshot, analysis, Path.Combine(outputDir, "summary.txt"), resolver);
	}

	private static StaleMissingAnalysis AnalyzeShaderVsMaterial(VTDiagnosticsSnapshot snap, IVTAssetInfoResolver resolver)
	{
		StaleMissingAnalysis staleMissingAnalysis = new StaleMissingAnalysis();
		Dictionary<int, EntryRecord> dictionary = new Dictionary<int, EntryRecord>(snap.Entries.Count);
		foreach (EntryRecord entry in snap.Entries)
		{
			dictionary[entry.IndexInAllocator] = entry;
		}
		foreach (MaterialRecord material in snap.Materials)
		{
			if (!material.MaterialAlive)
			{
				continue;
			}
			if (material.ShaderDeclaredLayerMasks == null)
			{
				staleMissingAnalysis.MaterialsWithNoShaderInfo++;
				continue;
			}
			string materialResolved = resolver.ResolveMaterialInstance(material.InstanceId);
			string key = material.ShaderName ?? "<null>";
			for (int i = 0; i < material.RegisteredCount; i++)
			{
				int num = material.RegisteredIndices[i];
				if (num < 0 || !dictionary.TryGetValue(num, out var value))
				{
					continue;
				}
				int num2 = ((i < material.ShaderDeclaredLayerMasks.Length) ? material.ShaderDeclaredLayerMasks[i] : 0);
				if (num2 == 0)
				{
					continue;
				}
				int num3 = (int)((value.PackedLayerFlags >> 8) & 0xF);
				int num4 = num3 & ~num2;
				int num5 = num2 & ~num3;
				if (num4 != 0)
				{
					staleMissingAnalysis.StackPairsWithExtras++;
					staleMissingAnalysis.StalePerShader.TryGetValue(key, out var value2);
					staleMissingAnalysis.StalePerShader[key] = value2 + 1;
				}
				if (num5 != 0)
				{
					staleMissingAnalysis.StackPairsWithMissing++;
					staleMissingAnalysis.MissingPerShader.TryGetValue(key, out var value3);
					staleMissingAnalysis.MissingPerShader[key] = value3 + 1;
				}
				for (int j = 0; j < 4; j++)
				{
					int num6 = 1 << j;
					if ((num4 & num6) != 0)
					{
						staleMissingAnalysis.Stale.Add(new StaleTagInfo
						{
							MaterialInstanceId = material.InstanceId,
							MaterialResolved = materialResolved,
							ShaderName = material.ShaderName,
							LocalStackIndex = i,
							EntryIndex = num,
							Slot = j,
							Guid = GetEntryLayerGuid(value, j)
						});
					}
					if ((num5 & num6) != 0)
					{
						staleMissingAnalysis.Missing.Add(new StaleTagInfo
						{
							MaterialInstanceId = material.InstanceId,
							MaterialResolved = materialResolved,
							ShaderName = material.ShaderName,
							LocalStackIndex = i,
							EntryIndex = num,
							Slot = j,
							Guid = GetEntryLayerGuid(value, j)
						});
					}
				}
			}
		}
		return staleMissingAnalysis;
	}

	private static Guid GetEntryLayerGuid(EntryRecord e, int slot)
	{
		return slot switch
		{
			0 => e.Layer0, 
			1 => e.Layer1, 
			2 => e.Layer2, 
			3 => e.Layer3, 
			_ => Guid.Empty, 
		};
	}

	private static void CaptureEntries(VirtualAtlas atlas, VTDiagnosticsSnapshot snap)
	{
		NativeList<VirtualAtlasEntry> entries = atlas.Entries;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < entries.Length; i++)
		{
			VirtualAtlasEntry virtualAtlasEntry = entries[i];
			if (virtualAtlasEntry.NodeKind == NodeKind.Alloc)
			{
				num++;
			}
			else
			{
				num2++;
			}
			snap.Entries.Add(new EntryRecord
			{
				IndexInAllocator = i,
				NodeKind = virtualAtlasEntry.NodeKind.ToString(),
				Layer0 = virtualAtlasEntry.StackId.Layer0,
				Layer1 = virtualAtlasEntry.StackId.Layer1,
				Layer2 = virtualAtlasEntry.StackId.Layer2,
				Layer3 = virtualAtlasEntry.StackId.Layer3,
				RectX = virtualAtlasEntry.RectInTiles.x,
				RectY = virtualAtlasEntry.RectInTiles.y,
				RectW = virtualAtlasEntry.RectInTiles.z,
				RectH = virtualAtlasEntry.RectInTiles.w,
				TextureSizeInTilesX = virtualAtlasEntry.TextureSizeInTiles.x,
				TextureSizeInTilesY = virtualAtlasEntry.TextureSizeInTiles.y,
				MipCount = virtualAtlasEntry.MipCount,
				MipBias = virtualAtlasEntry.MipBias,
				LayerCount = virtualAtlasEntry.LayerCount,
				PackedLayerFlags = virtualAtlasEntry.PackedLayerFlags,
				RectAllocId = virtualAtlasEntry.RectAllocId
			});
		}
		snap.AllocatedEntriesCount = num;
		snap.FreeEntriesCount = num2;
	}

	private static void CaptureMaterials(VirtualAtlas atlas, VTDiagnosticsSnapshot snap)
	{
		NativeKeyValueArrays<int, MaterialStackIndices> nativeKeyValueArrays = default(NativeKeyValueArrays<int, MaterialStackIndices>);
		try
		{
			nativeKeyValueArrays = atlas.MaterialStackIndices.GetKeyValueArrays(Allocator.Temp);
			snap.RegisteredMaterialsCount = nativeKeyValueArrays.Keys.Length;
			List<UnityEngine.Object> list = new List<UnityEngine.Object>(nativeKeyValueArrays.Keys.Length);
			Resources.InstanceIDToObjectList(nativeKeyValueArrays.Keys, list);
			int vTStackIndices = ShaderPropertyId._VTStackIndices;
			for (int i = 0; i < nativeKeyValueArrays.Keys.Length; i++)
			{
				int instanceId = nativeKeyValueArrays.Keys[i];
				MaterialStackIndices materialStackIndices = nativeKeyValueArrays.Values[i];
				int num = math.clamp(materialStackIndices.Count, 0, 16);
				int[] array = new int[num];
				for (int j = 0; j < num; j++)
				{
					array[j] = materialStackIndices[j];
				}
				Material material = ((i < list.Count) ? (list[i] as Material) : null);
				float[] array2 = null;
				int[] counts = null;
				int[] masks = null;
				string shaderName = null;
				if (material != null)
				{
					if (material.HasMatrix(vTStackIndices))
					{
						Matrix4x4 matrix = material.GetMatrix(vTStackIndices);
						array2 = new float[16];
						for (int k = 0; k < 4; k++)
						{
							for (int l = 0; l < 4; l++)
							{
								array2[k * 4 + l] = matrix[k, l];
							}
						}
					}
					if (material.shader != null)
					{
						shaderName = material.shader.name;
						ExtractShaderVTLayerInfo(material.shader, out counts, out masks);
					}
				}
				snap.Materials.Add(new MaterialRecord
				{
					InstanceId = instanceId,
					MaterialAlive = (material != null),
					RegisteredCount = num,
					RegisteredIndices = array,
					ActualMatrix16 = array2,
					ShaderDeclaredLayerCounts = counts,
					ShaderDeclaredLayerMasks = masks,
					ShaderName = shaderName
				});
			}
		}
		finally
		{
			if (nativeKeyValueArrays.Keys.IsCreated)
			{
				nativeKeyValueArrays.Dispose();
			}
		}
	}

	private static void CapturePages(VirtualAtlas atlas, VTDiagnosticsSnapshot snap, Dictionary<int, int> entryMipCount)
	{
		NativeArray<Page> pages = atlas.Pages;
		int x = atlas.ResolutionInTiles.x;
		if (x <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < pages.Length; i++)
		{
			Page page = pages[i];
			if (page.TextureId != -1 || page.IsLoading)
			{
				if (page.TextureId >= 0 && entryMipCount.TryGetValue(page.TextureId, out var value) && page.MipLevel >= value)
				{
					num++;
					continue;
				}
				int virtualX = i % x;
				int virtualY = i / x;
				snap.Pages.Add(new PageRecord
				{
					VirtualX = virtualX,
					VirtualY = virtualY,
					TextureId = page.TextureId,
					MipLevel = page.MipLevel,
					PhysX = page.PhysicalTileCoord.x,
					PhysY = page.PhysicalTileCoord.y,
					PhysSlice = page.PhysicalTileCoord.z,
					IsLoading = page.IsLoading,
					FrameId = page.FrameId,
					IsReady = page.IsReady
				});
			}
		}
		snap.PyramidCornerSkippedPages = num;
	}

	private static int[] ExtractShaderVTLayerCounts(Shader shader)
	{
		ExtractShaderVTLayerInfo(shader, out var counts, out var _);
		return counts;
	}

	private static int[] ExtractShaderVTLayerMasks(Shader shader)
	{
		ExtractShaderVTLayerInfo(shader, out var _, out var masks);
		return masks;
	}

	private static void ExtractShaderVTLayerInfo(Shader shader, out int[] counts, out int[] masks)
	{
		counts = null;
		masks = null;
		int propertyCount = shader.GetPropertyCount();
		Dictionary<int, int> dictionary = new Dictionary<int, int>(4);
		int num = -1;
		for (int i = 0; i < propertyCount; i++)
		{
			string[] propertyAttributes = shader.GetPropertyAttributes(i);
			if (propertyAttributes == null)
			{
				continue;
			}
			string[] array = propertyAttributes;
			for (int j = 0; j < array.Length; j++)
			{
				if (VirtualTextureUtils.TryParseVTAttribute(array[j], out var localStackId, out var layerIndex) && localStackId >= 0 && localStackId <= 15 && layerIndex >= 0 && layerIndex <= 3)
				{
					dictionary.TryGetValue(localStackId, out var value);
					dictionary[localStackId] = value | (1 << layerIndex);
					if (localStackId > num)
					{
						num = localStackId;
					}
				}
			}
		}
		if (num < 0)
		{
			return;
		}
		masks = new int[num + 1];
		counts = new int[num + 1];
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			masks[item.Key] = item.Value;
			int num2 = 0;
			int num3 = item.Value;
			while (num3 != 0)
			{
				num3 >>= 1;
				num2++;
			}
			counts[item.Key] = num2;
		}
	}

	private static void CapturePhysicalToVirtual(VirtualAtlas atlas, VTDiagnosticsSnapshot snap)
	{
		NativeKeyValueArrays<int3, int2> nativeKeyValueArrays = default(NativeKeyValueArrays<int3, int2>);
		try
		{
			nativeKeyValueArrays = atlas.PhysicalToVirtualPageMap.GetKeyValueArrays(Allocator.Temp);
			for (int i = 0; i < nativeKeyValueArrays.Keys.Length; i++)
			{
				int3 @int = nativeKeyValueArrays.Keys[i];
				int2 int2 = nativeKeyValueArrays.Values[i];
				snap.PhysicalToVirtual.Add(new PhysicalToVirtualRecord
				{
					PhysX = @int.x,
					PhysY = @int.y,
					PhysSlice = @int.z,
					VirtX = int2.x,
					VirtY = int2.y
				});
			}
		}
		finally
		{
			if (nativeKeyValueArrays.Keys.IsCreated)
			{
				nativeKeyValueArrays.Dispose();
			}
		}
	}

	private static void CaptureGpuBuffer(VirtualAtlas atlas, VTDiagnosticsSnapshot snap)
	{
		GraphicsBuffer textureStackDataBuffer = atlas.TextureStackDataBuffer;
		if (textureStackDataBuffer != null && textureStackDataBuffer.count > 0)
		{
			uint[] array = new uint[textureStackDataBuffer.count];
			textureStackDataBuffer.GetData(array);
			int num = array.Length / 4;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 4;
				uint num3 = array[num2];
				uint num4 = array[num2 + 1];
				uint num5 = array[num2 + 2];
				uint num6 = array[num2 + 3];
				int rectX = (int)(num3 & 0xFFFF);
				int rectY = (int)((num3 >> 16) & 0xFFFF);
				int rectW = (int)(num4 & 0xFFFF);
				int rectH = (int)((num4 >> 16) & 0xFFFF);
				int maxMip = (int)(num5 & 0xFFFF);
				float mipBias = (float)(int)(((num5 >> 16) & 0xFFFF) - 32767) / 32767f * 16383f;
				int layerCount = (int)(num6 & 0xF);
				int packedLayerFlags = (int)((num6 >> 4) & 0xFFF);
				int textureWidthInTiles = (int)((num6 >> 16) & 0xFFFF);
				snap.GpuEntries.Add(new GpuEntryRecord
				{
					EntryIndex = i,
					RectX = rectX,
					RectY = rectY,
					RectW = rectW,
					RectH = rectH,
					MaxMip = maxMip,
					MipBias = mipBias,
					LayerCount = layerCount,
					PackedLayerFlags = packedLayerFlags,
					TextureWidthInTiles = textureWidthInTiles
				});
			}
		}
	}

	private static void CaptureIndirectionDrawData(VirtualTextureManager vtm, VTDiagnosticsSnapshot snap)
	{
	}

	private static void CaptureIndirection(VirtualTextureManager vtm, VTDiagnosticsSnapshot snap)
	{
		RTHandle indirectTexture = vtm.IndirectTexture;
		if (indirectTexture == null || indirectTexture.rt == null)
		{
			return;
		}
		RenderTexture rt = indirectTexture.rt;
		snap.IndirectionWidth = rt.width;
		snap.IndirectionHeight = rt.height;
		Texture2D texture2D = null;
		RenderTexture active = RenderTexture.active;
		try
		{
			texture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, mipChain: false, linear: true);
			RenderTexture.active = rt;
			texture2D.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			snap.IndirectionPng = texture2D.EncodeToPNG();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[VT Diagnostics] Indirection readback failed: " + ex.Message);
		}
		finally
		{
			RenderTexture.active = active;
			if (texture2D != null)
			{
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
		}
	}

	private static void WriteEntriesJson(VTDiagnosticsSnapshot snap, string path, IVTAssetInfoResolver resolver)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.PropArray("entries");
		foreach (EntryRecord entry in snap.Entries)
		{
			jsonOut.StartObject();
			jsonOut.Prop("indexInAllocator", entry.IndexInAllocator);
			jsonOut.Prop("nodeKind", entry.NodeKind);
			jsonOut.PropArray("layers");
			AppendLayer(jsonOut, entry.Layer0, resolver);
			AppendLayer(jsonOut, entry.Layer1, resolver);
			AppendLayer(jsonOut, entry.Layer2, resolver);
			AppendLayer(jsonOut, entry.Layer3, resolver);
			jsonOut.EndArray();
			jsonOut.PropArray("rectInTiles");
			jsonOut.Value(entry.RectX);
			jsonOut.Value(entry.RectY);
			jsonOut.Value(entry.RectW);
			jsonOut.Value(entry.RectH);
			jsonOut.EndArray();
			jsonOut.PropArray("textureSizeInTiles");
			jsonOut.Value(entry.TextureSizeInTilesX);
			jsonOut.Value(entry.TextureSizeInTilesY);
			jsonOut.EndArray();
			jsonOut.Prop("mipCount", entry.MipCount);
			jsonOut.Prop("mipBias", entry.MipBias);
			jsonOut.Prop("layerCount", entry.LayerCount);
			jsonOut.Prop("packedLayerFlags", (int)entry.PackedLayerFlags);
			jsonOut.Prop("rectAllocId", (int)entry.RectAllocId);
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void AppendLayer(JsonOut j, Guid g, IVTAssetInfoResolver resolver)
	{
		j.StartObject();
		j.Prop("guid", (g == Guid.Empty) ? "<empty>" : g.ToString("N"));
		string text = resolver.ResolveTextureGuid(g);
		if (!string.IsNullOrEmpty(text))
		{
			j.Prop("info", text);
		}
		j.EndObject();
	}

	private static void WriteMaterialsJson(VTDiagnosticsSnapshot snap, string path, IVTAssetInfoResolver resolver)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.PropArray("materials");
		foreach (MaterialRecord material in snap.Materials)
		{
			jsonOut.StartObject();
			jsonOut.Prop("instanceId", material.InstanceId);
			jsonOut.Prop("alive", material.MaterialAlive);
			string text = resolver.ResolveMaterialInstance(material.InstanceId);
			if (!string.IsNullOrEmpty(text))
			{
				jsonOut.Prop("info", text);
			}
			if (!string.IsNullOrEmpty(material.ShaderName))
			{
				jsonOut.Prop("shaderName", material.ShaderName);
			}
			jsonOut.Prop("registeredCount", material.RegisteredCount);
			jsonOut.PropArray("registeredIndices");
			if (material.RegisteredIndices != null)
			{
				int[] registeredIndices = material.RegisteredIndices;
				foreach (int v in registeredIndices)
				{
					jsonOut.Value(v);
				}
			}
			jsonOut.EndArray();
			if (material.ShaderDeclaredLayerCounts != null)
			{
				jsonOut.PropArray("shaderDeclaredLayerCounts");
				int[] registeredIndices = material.ShaderDeclaredLayerCounts;
				foreach (int v2 in registeredIndices)
				{
					jsonOut.Value(v2);
				}
				jsonOut.EndArray();
			}
			if (material.ShaderDeclaredLayerMasks != null)
			{
				jsonOut.PropArray("shaderDeclaredLayerMasks");
				int[] registeredIndices = material.ShaderDeclaredLayerMasks;
				foreach (int v3 in registeredIndices)
				{
					jsonOut.Value(v3);
				}
				jsonOut.EndArray();
			}
			if (material.ActualMatrix16 != null)
			{
				jsonOut.PropArray("actualMatrix16");
				float[] actualMatrix = material.ActualMatrix16;
				foreach (float v4 in actualMatrix)
				{
					jsonOut.Value(v4);
				}
				jsonOut.EndArray();
			}
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void WritePagesJson(VTDiagnosticsSnapshot snap, string path)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.Prop("note", "Only pages with TextureId != -1 or IsLoading are listed.");
		jsonOut.PropArray("pages");
		foreach (PageRecord page in snap.Pages)
		{
			jsonOut.StartObject();
			jsonOut.PropArray("virtualCoord");
			jsonOut.Value(page.VirtualX);
			jsonOut.Value(page.VirtualY);
			jsonOut.EndArray();
			jsonOut.Prop("textureId", page.TextureId);
			jsonOut.Prop("mipLevel", page.MipLevel);
			jsonOut.PropArray("physicalTileCoord");
			jsonOut.Value(page.PhysX);
			jsonOut.Value(page.PhysY);
			jsonOut.Value(page.PhysSlice);
			jsonOut.EndArray();
			jsonOut.Prop("isLoading", page.IsLoading);
			jsonOut.Prop("isReady", page.IsReady);
			jsonOut.Prop("frameId", page.FrameId);
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void WriteGpuBufferJson(VTDiagnosticsSnapshot snap, string path)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.Prop("note", "Parsed from _VTTextureStackDataBuffer (4 uints per entry, layout per VTInclude.hlsl).");
		jsonOut.PropArray("entries");
		foreach (GpuEntryRecord gpuEntry in snap.GpuEntries)
		{
			jsonOut.StartObject();
			jsonOut.Prop("entryIndex", gpuEntry.EntryIndex);
			jsonOut.PropArray("rectInTiles");
			jsonOut.Value(gpuEntry.RectX);
			jsonOut.Value(gpuEntry.RectY);
			jsonOut.Value(gpuEntry.RectW);
			jsonOut.Value(gpuEntry.RectH);
			jsonOut.EndArray();
			jsonOut.Prop("maxMip", gpuEntry.MaxMip);
			jsonOut.Prop("mipBias", gpuEntry.MipBias);
			jsonOut.Prop("layerCount", gpuEntry.LayerCount);
			jsonOut.Prop("packedLayerFlags", gpuEntry.PackedLayerFlags);
			jsonOut.Prop("textureWidthInTiles", gpuEntry.TextureWidthInTiles);
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void WritePhysicalToVirtualJson(VTDiagnosticsSnapshot snap, string path)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.PropArray("entries");
		foreach (PhysicalToVirtualRecord item in snap.PhysicalToVirtual)
		{
			jsonOut.StartObject();
			jsonOut.PropArray("physicalCoord");
			jsonOut.Value(item.PhysX);
			jsonOut.Value(item.PhysY);
			jsonOut.Value(item.PhysSlice);
			jsonOut.EndArray();
			jsonOut.PropArray("virtualCoord");
			jsonOut.Value(item.VirtX);
			jsonOut.Value(item.VirtY);
			jsonOut.EndArray();
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void WriteIndirectionDrawDataJson(VTDiagnosticsSnapshot snap, string path)
	{
		JsonOut jsonOut = new JsonOut();
		jsonOut.StartObject();
		jsonOut.Prop("note", "Quads emitted by IndirectionTextureRenderer for the captured frame (one PageDrawData per source page * (page.mipLevel+1) target mips).");
		jsonOut.Prop("count", snap.IndirectionDrawData.Count);
		jsonOut.PropArray("quads");
		foreach (IndirectionDrawRecord indirectionDrawDatum in snap.IndirectionDrawData)
		{
			jsonOut.StartObject();
			jsonOut.Prop("mipLevel", indirectionDrawDatum.MipLevel);
			jsonOut.PropArray("rect");
			jsonOut.Value(indirectionDrawDatum.RectX);
			jsonOut.Value(indirectionDrawDatum.RectY);
			jsonOut.Value(indirectionDrawDatum.RectW);
			jsonOut.Value(indirectionDrawDatum.RectH);
			jsonOut.EndArray();
			jsonOut.PropArray("drawPos");
			jsonOut.Value(indirectionDrawDatum.DrawPosX);
			jsonOut.Value(indirectionDrawDatum.DrawPosY);
			jsonOut.EndArray();
			jsonOut.Prop("sliceIndex", indirectionDrawDatum.SliceIndex);
			jsonOut.EndObject();
		}
		jsonOut.EndArray();
		jsonOut.EndObject();
		File.WriteAllText(path, jsonOut.ToString());
	}

	private static void WriteIndirectionPng(VTDiagnosticsSnapshot snap, string path)
	{
		if (snap.IndirectionPng != null && snap.IndirectionPng.Length != 0)
		{
			File.WriteAllBytes(path, snap.IndirectionPng);
		}
	}

	private static void WriteSummary(VTDiagnosticsSnapshot snap, StaleMissingAnalysis analysis, string path, IVTAssetInfoResolver resolver)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== VT Diagnostics Summary ===");
		stringBuilder.AppendLine($"Captured at (UTC): {snap.CapturedAtUtc:yyyy-MM-dd HH:mm:ss}");
		stringBuilder.AppendLine($"Frame id: {snap.FrameId}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"Atlas resolution (tiles): {snap.AtlasResolutionInTiles.x} x {snap.AtlasResolutionInTiles.y}");
		stringBuilder.AppendLine($"Allocator size (tiles):   {snap.AllocatorWidth} x {snap.AllocatorHeight}");
		stringBuilder.AppendLine($"Occupancy:                {snap.Occupancy:P1}");
		stringBuilder.AppendLine($"Max mip count:            {snap.MaxMipCount}");
		stringBuilder.AppendLine($"Entries:                  {snap.AllocatedEntriesCount} allocated, {snap.FreeEntriesCount} free");
		stringBuilder.AppendLine($"Materials registered:     {snap.RegisteredMaterialsCount}");
		stringBuilder.AppendLine($"Physical atlas:           {snap.PhysicalAtlasTilesInSliceX}x{snap.PhysicalAtlasTilesInSliceY} tiles x {snap.PhysicalAtlasSliceCount} slices");
		stringBuilder.AppendLine($"Indirection texture:      {snap.IndirectionWidth} x {snap.IndirectionHeight}");
		stringBuilder.AppendLine($"Indirection last Render:  frame {snap.IndirectionLastRenderFrameId} (captured at frame {snap.FrameId}, delta {snap.FrameId - snap.IndirectionLastRenderFrameId}; last quads drawn: {snap.IndirectionLastRenderQuadCount})");
		stringBuilder.AppendLine($"Pages skipped (invalid pyramid corners): {snap.PyramidCornerSkippedPages}");
		stringBuilder.AppendLine();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		EmitShaderVsMaterialSummary(analysis, list);
		CrossCheckDuplicateLayerGuids(snap, resolver, list, list2);
		CrossCheckStackIndicesVsMatrix(snap, resolver, list, list2);
		CrossCheckCpuVsGpu(snap, list, list2);
		CrossCheckOverlap(snap, resolver, list, list2);
		CrossCheckMipPyramidWidth(snap, resolver, list, list2);
		CrossCheckPagesVsP2V(snap, list);
		stringBuilder.AppendLine("=== Cross-checks (smoking guns first) ===");
		foreach (string item in list)
		{
			stringBuilder.AppendLine(item);
		}
		stringBuilder.AppendLine();
		if (analysis.StackPairsWithExtras > 0)
		{
			stringBuilder.AppendLine("=== Per-shader breakdown: STALE EXTRA tags (Pattern B - direct bug source) ===");
			EmitPerShaderBreakdown(analysis.StalePerShader, stringBuilder);
			stringBuilder.AppendLine("Full list with material paths: stale_tags.txt");
			stringBuilder.AppendLine();
		}
		if (analysis.StackPairsWithMissing > 0)
		{
			stringBuilder.AppendLine("=== Per-shader breakdown: MISSING optional tags (Pattern A - usually OK) ===");
			EmitPerShaderBreakdown(analysis.MissingPerShader, stringBuilder);
			stringBuilder.AppendLine("Full list with material paths: missing_tags.txt");
			stringBuilder.AppendLine();
		}
		if (list2.Count > 0)
		{
			stringBuilder.AppendLine($"=== Detailed findings (first {50} of each kind; full lists in *.txt files where applicable) ===");
			int num = 0;
			foreach (string item2 in list2)
			{
				stringBuilder.AppendLine(item2);
				if (++num >= 200)
				{
					break;
				}
			}
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	private static void EmitShaderVsMaterialSummary(StaleMissingAnalysis analysis, List<string> lines)
	{
		if (analysis.StackPairsWithExtras > 0)
		{
			lines.Add($"[ERR-SUMMARY] {analysis.StackPairsWithExtras} material/stack pairs with STALE EXTRA tags (entry presence bits > shader-declared mask). Direct bug source. {analysis.Stale.Count} stale layer slots total. See stale_tags.txt.");
		}
		if (analysis.StackPairsWithMissing > 0)
		{
			lines.Add($"[INFO-SUMMARY] {analysis.StackPairsWithMissing} material/stack pairs with MISSING optional layers (shader-declared mask > entry presence bits). {analysis.Missing.Count} missing layer slots total. Probably OK. See missing_tags.txt.");
		}
		if (analysis.MaterialsWithNoShaderInfo > 0)
		{
			lines.Add($"[INFO] {analysis.MaterialsWithNoShaderInfo} materials skipped (shader didn't expose VT attributes — non-VT shader or destroyed).");
		}
		if (analysis.StackPairsWithExtras == 0 && analysis.StackPairsWithMissing == 0)
		{
			lines.Add("[OK] All material/stack pairs have entry layer presence == shader-declared mask.");
		}
	}

	private static void EmitPerShaderBreakdown(Dictionary<string, int> map, StringBuilder sb)
	{
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(map);
		list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => b.Value.CompareTo(a.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			sb.AppendLine($"  {item.Value,5}  {item.Key}");
		}
	}

	private static void WriteStaleTagsList(StaleMissingAnalysis analysis, string path, IVTAssetInfoResolver resolver)
	{
		if (analysis.Stale.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("# Stale VT material tags — VT_STACK_X_LAYER_Y tag is set on material, but shader does not declare [VirtualTexture(X, Y)].");
		stringBuilder.AppendLine("# Format: <slot> <material> <shader> stack=<stackIdx> entry=<entryIdx> guid=<guid> texture=<resolved-path>");
		stringBuilder.AppendLine();
		foreach (StaleTagInfo item in analysis.Stale)
		{
			string text = resolver.ResolveTextureGuid(item.Guid);
			object[] obj = new object[7] { item.Slot, null, null, null, null, null, null };
			string text2 = item.MaterialResolved;
			if (text2 == null)
			{
				int materialInstanceId = item.MaterialInstanceId;
				text2 = materialInstanceId.ToString();
			}
			obj[1] = text2;
			obj[2] = item.ShaderName;
			obj[3] = item.LocalStackIndex;
			obj[4] = item.EntryIndex;
			obj[5] = item.Guid;
			obj[6] = text;
			stringBuilder.AppendLine(string.Format("slot{0}\t{1}\tshader='{2}'\tstack={3}\tentry={4}\tguid={5:N}\ttexture={6}", obj));
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	private static void WriteMissingTagsList(StaleMissingAnalysis analysis, string path, IVTAssetInfoResolver resolver)
	{
		if (analysis.Missing.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("# Missing VT material tags — shader declares [VirtualTexture(X, Y)] but material's VT_STACK_X_LAYER_Y is missing/empty or its texture has no .tiledtexture file.");
		stringBuilder.AppendLine("# Often legitimate (optional shader layer not used by this material).");
		stringBuilder.AppendLine("# Format: <slot> <material> <shader> stack=<stackIdx> entry=<entryIdx> guid=<guid>");
		stringBuilder.AppendLine();
		foreach (StaleTagInfo item in analysis.Missing)
		{
			object[] obj = new object[6] { item.Slot, null, null, null, null, null };
			string text = item.MaterialResolved;
			if (text == null)
			{
				int materialInstanceId = item.MaterialInstanceId;
				text = materialInstanceId.ToString();
			}
			obj[1] = text;
			obj[2] = item.ShaderName;
			obj[3] = item.LocalStackIndex;
			obj[4] = item.EntryIndex;
			obj[5] = item.Guid;
			stringBuilder.AppendLine(string.Format("slot{0}\t{1}\tshader='{2}'\tstack={3}\tentry={4}\tguid={5:N}", obj));
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	private static void CrossCheckStackIndicesVsMatrix(VTDiagnosticsSnapshot snap, IVTAssetInfoResolver resolver, List<string> headLines, List<string> detailLines)
	{
		int num = 0;
		foreach (MaterialRecord material in snap.Materials)
		{
			if (!material.MaterialAlive || material.ActualMatrix16 == null)
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < material.RegisteredCount; i++)
			{
				float num2 = material.ActualMatrix16[i];
				int num3 = material.RegisteredIndices[i];
				if ((int)num2 != num3)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			num++;
			if (detailLines.Count >= 200)
			{
				continue;
			}
			string text = resolver.ResolveMaterialInstance(material.InstanceId);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"[WARN] Material instanceId={material.InstanceId}");
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(" (" + text + ")");
			}
			stringBuilder.Append(": registered=[");
			for (int j = 0; j < material.RegisteredCount; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(material.RegisteredIndices[j]);
			}
			stringBuilder.Append("] actual=[");
			for (int k = 0; k < material.RegisteredCount; k++)
			{
				if (k > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append((int)material.ActualMatrix16[k]);
			}
			stringBuilder.Append("]");
			detailLines.Add(stringBuilder.ToString());
		}
		if (num == 0)
		{
			headLines.Add("[OK] All registered material _VTStackIndices match what is set on the material.");
		}
		else
		{
			headLines.Add($"[WARN-SUMMARY] {num} materials have stale _VTStackIndices vs MaterialStackIndices registration (H1/H2 indicator).");
		}
	}

	private static void CrossCheckCpuVsGpu(VTDiagnosticsSnapshot snap, List<string> headLines, List<string> detailLines)
	{
		int num = 0;
		Dictionary<int, GpuEntryRecord> dictionary = new Dictionary<int, GpuEntryRecord>(snap.GpuEntries.Count);
		foreach (GpuEntryRecord gpuEntry in snap.GpuEntries)
		{
			dictionary[gpuEntry.EntryIndex] = gpuEntry;
		}
		foreach (EntryRecord entry in snap.Entries)
		{
			if (entry.NodeKind != "Alloc")
			{
				continue;
			}
			if (!dictionary.TryGetValue(entry.IndexInAllocator, out var value))
			{
				if (detailLines.Count < 200)
				{
					detailLines.Add($"[ERR ] Entry #{entry.IndexInAllocator} (Alloc) has no GPU buffer counterpart.");
				}
				num++;
				continue;
			}
			if (value.RectX != entry.RectX || value.RectY != entry.RectY || value.RectW != entry.RectW || value.RectH != entry.RectH)
			{
				if (detailLines.Count < 200)
				{
					detailLines.Add($"[ERR ] Entry #{entry.IndexInAllocator}: rect CPU=({entry.RectX},{entry.RectY},{entry.RectW},{entry.RectH}) GPU=({value.RectX},{value.RectY},{value.RectW},{value.RectH}).");
				}
				num++;
			}
			int num2 = entry.MipCount - 1;
			if (value.MaxMip != num2)
			{
				if (detailLines.Count < 200)
				{
					detailLines.Add($"[ERR ] Entry #{entry.IndexInAllocator}: maxMip CPU={num2} GPU={value.MaxMip}.");
				}
				num++;
			}
			if (value.LayerCount != entry.LayerCount)
			{
				if (detailLines.Count < 200)
				{
					detailLines.Add($"[ERR ] Entry #{entry.IndexInAllocator}: layerCount CPU={entry.LayerCount} GPU={value.LayerCount}.");
				}
				num++;
			}
			if (value.PackedLayerFlags != (int)entry.PackedLayerFlags)
			{
				if (detailLines.Count < 200)
				{
					detailLines.Add($"[ERR ] Entry #{entry.IndexInAllocator}: packedLayerFlags CPU={entry.PackedLayerFlags} GPU={value.PackedLayerFlags}.");
				}
				num++;
			}
		}
		if (num == 0)
		{
			headLines.Add("[OK] _VTTextureStackDataBuffer matches CPU entries.");
		}
		else
		{
			headLines.Add($"[ERR-SUMMARY] {num} CPU<->GPU buffer mismatches detected.");
		}
	}

	private static void CrossCheckOverlap(VTDiagnosticsSnapshot snap, IVTAssetInfoResolver resolver, List<string> headLines, List<string> detailLines)
	{
		int num = 0;
		List<EntryRecord> list = new List<EntryRecord>(snap.AllocatedEntriesCount);
		foreach (EntryRecord entry in snap.Entries)
		{
			if (entry.NodeKind == "Alloc")
			{
				list.Add(entry);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				EntryRecord a = list[i];
				EntryRecord b = list[j];
				if (RectsOverlap(a, b))
				{
					num++;
					if (detailLines.Count < 200)
					{
						string text = resolver.ResolveTextureGuid(a.Layer0);
						string text2 = resolver.ResolveTextureGuid(b.Layer0);
						detailLines.Add($"[ERR ] Rect overlap: entry #{a.IndexInAllocator} ({text}) rect=({a.RectX},{a.RectY},{a.RectW},{a.RectH}) vs entry #{b.IndexInAllocator} ({text2}) rect=({b.RectX},{b.RectY},{b.RectW},{b.RectH}).");
					}
				}
			}
		}
		if (num == 0)
		{
			headLines.Add("[OK] No rect overlaps between allocated entries.");
		}
		else
		{
			headLines.Add($"[ERR-SUMMARY] {num} allocator rect overlap(s) detected (H12 indicator).");
		}
	}

	private static bool RectsOverlap(EntryRecord a, EntryRecord b)
	{
		int num = a.RectX + a.RectW;
		int num2 = a.RectY + a.RectH;
		int num3 = b.RectX + b.RectW;
		int num4 = b.RectY + b.RectH;
		if (a.RectX < num3 && b.RectX < num && a.RectY < num4)
		{
			return b.RectY < num2;
		}
		return false;
	}

	private static void CrossCheckMipPyramidWidth(VTDiagnosticsSnapshot snap, IVTAssetInfoResolver resolver, List<string> headLines, List<string> detailLines)
	{
		int num = 0;
		foreach (EntryRecord entry in snap.Entries)
		{
			if (entry.NodeKind != "Alloc" || entry.MipCount <= 1)
			{
				continue;
			}
			int textureSizeInTilesX = entry.TextureSizeInTilesX;
			int num2 = (int)((float)entry.RectW / 1.5f);
			if (num2 != textureSizeInTilesX)
			{
				num++;
				if (detailLines.Count < 200)
				{
					string text = resolver.ResolveTextureGuid(entry.Layer0);
					detailLines.Add($"[ERR ] Mip pyramid width mismatch: entry #{entry.IndexInAllocator} ({text}) rect.w={entry.RectW} -> decoded textureWidth={num2} but TextureSizeInTiles.x={textureSizeInTilesX}. Shader will sample wrong pyramid offset on mip > 0.");
				}
			}
		}
		if (num == 0)
		{
			headLines.Add("[OK] Mip pyramid widths consistent (rect.w/1.5 == TextureSizeInTiles.x for all allocated mipmapped entries).");
		}
		else
		{
			headLines.Add($"[ERR-SUMMARY] {num} entries with mip pyramid width mismatch — direct cause of mip>0 wrong tile sampling.");
		}
	}

	private static void CrossCheckDuplicateLayerGuids(VTDiagnosticsSnapshot snap, IVTAssetInfoResolver resolver, List<string> headLines, List<string> detailLines)
	{
		int num = 0;
		foreach (EntryRecord entry in snap.Entries)
		{
			if (entry.NodeKind != "Alloc")
			{
				continue;
			}
			Guid[] array = new Guid[4] { entry.Layer0, entry.Layer1, entry.Layer2, entry.Layer3 };
			bool flag = false;
			for (int i = 0; i < 4; i++)
			{
				if (flag)
				{
					break;
				}
				if (array[i] == Guid.Empty)
				{
					continue;
				}
				for (int j = i + 1; j < 4; j++)
				{
					if (!(array[j] == Guid.Empty) && array[i] == array[j])
					{
						num++;
						flag = true;
						if (detailLines.Count < 200)
						{
							string text = resolver.ResolveTextureGuid(array[i]);
							detailLines.Add($"[WARN] Entry #{entry.IndexInAllocator}: layer{i} == layer{j} ({text}). Same texture in two layer slots — possibly stale duplicate VT_STACK_*_LAYER_* tag.");
						}
						break;
					}
				}
			}
		}
		if (num > 0)
		{
			headLines.Add($"[WARN-SUMMARY] {num} entries with identical GUIDs across layers — usually stale tag indicator.");
		}
		else
		{
			headLines.Add("[OK] No entries with duplicate GUIDs across layers.");
		}
	}

	private static void CrossCheckPagesVsP2V(VTDiagnosticsSnapshot snap, List<string> headLines)
	{
		Dictionary<long, (int, int)> dictionary = new Dictionary<long, (int, int)>(snap.PhysicalToVirtual.Count);
		foreach (PhysicalToVirtualRecord item in snap.PhysicalToVirtual)
		{
			dictionary[PackInt3(item.PhysX, item.PhysY, item.PhysSlice)] = (item.VirtX, item.VirtY);
		}
		int num = 0;
		int num2 = 0;
		foreach (PageRecord page in snap.Pages)
		{
			if (page.IsReady)
			{
				long key = PackInt3(page.PhysX, page.PhysY, page.PhysSlice);
				if (!dictionary.TryGetValue(key, out var value))
				{
					num++;
				}
				else if (value.Item1 != page.VirtualX || value.Item2 != page.VirtualY)
				{
					num2++;
				}
			}
		}
		if (num > 0)
		{
			headLines.Add($"[WARN] {num} ready pages point to physical coords missing from PhysicalToVirtualPageMap.");
		}
		if (num2 > 0)
		{
			headLines.Add($"[ERR ] {num2} pages have inconsistent forward/reverse mapping with PhysicalToVirtualPageMap.");
		}
		if (num == 0 && num2 == 0)
		{
			headLines.Add("[OK] Page <-> PhysicalToVirtualPageMap mappings consistent.");
		}
	}

	private static long PackInt3(int x, int y, int z)
	{
		return (long)((uint)x | ((ulong)(uint)y << 21) | ((ulong)(uint)z << 42));
	}
}
