using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public sealed class VTDiagnosticsSnapshot
{
	public DateTime CapturedAtUtc;

	public int FrameId;

	public int2 AtlasResolutionInTiles;

	public int AllocatorWidth;

	public int AllocatorHeight;

	public int MaxMipCount;

	public float Occupancy;

	public int AllocatedEntriesCount;

	public int FreeEntriesCount;

	public int RegisteredMaterialsCount;

	public int PyramidCornerSkippedPages;

	public int PhysicalAtlasTilesInSliceX;

	public int PhysicalAtlasTilesInSliceY;

	public int PhysicalAtlasSliceCount;

	public readonly List<EntryRecord> Entries = new List<EntryRecord>();

	public readonly List<MaterialRecord> Materials = new List<MaterialRecord>();

	public readonly List<PageRecord> Pages = new List<PageRecord>();

	public readonly List<GpuEntryRecord> GpuEntries = new List<GpuEntryRecord>();

	public readonly List<PhysicalToVirtualRecord> PhysicalToVirtual = new List<PhysicalToVirtualRecord>();

	public readonly List<IndirectionDrawRecord> IndirectionDrawData = new List<IndirectionDrawRecord>();

	public int IndirectionWidth;

	public int IndirectionHeight;

	public byte[] IndirectionPng;

	public int IndirectionLastRenderFrameId;

	public int IndirectionLastRenderQuadCount;

	public readonly List<PaddingLoadRequestEventRecord> PaddingLoadRequestEvents = new List<PaddingLoadRequestEventRecord>();

	public int PaddingLoadRequestEventsTotalCount;

	public int PaddingLoadRequestEventsRingCapacity;
}
