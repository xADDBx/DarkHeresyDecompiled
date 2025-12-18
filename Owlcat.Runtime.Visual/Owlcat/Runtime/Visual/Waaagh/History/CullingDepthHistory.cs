using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.History;

public sealed class CullingDepthHistory : WaaaghHistoryItem
{
	private const string kName = "CullingDepthHistory";

	private const int kInvalidVersion = -1;

	private Hash128 m_DescKey;

	private RenderTextureDescriptor m_Descriptor;

	private int m_Id;

	private int m_Version;

	private static int CurrentVersion => Time.frameCount;

	public int2 Resolution { get; private set; }

	public int MipLevel { get; private set; }

	public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
	{
		base.OnCreate(owner, typeId);
		m_Id = MakeId(0u);
		m_Version = -1;
	}

	public RTHandle GetTexture()
	{
		return GetCurrentFrameRT(m_Id);
	}

	public override void Reset()
	{
		ReleaseHistoryFrameRT(m_Id);
		m_Descriptor.width = 0;
		m_Descriptor.height = 0;
		m_Descriptor.graphicsFormat = GraphicsFormat.None;
		m_Version = -1;
	}

	internal bool IsUsable()
	{
		if (m_Version != -1)
		{
			return m_Version == CurrentVersion - 1;
		}
		return false;
	}

	internal void UpdateVersion()
	{
		m_Version = CurrentVersion;
	}

	internal bool Update(RenderTextureDescriptor depthDesc)
	{
		MipLevel = FindMipLevel(ref depthDesc);
		if (depthDesc.width > 0 && depthDesc.height > 0 && depthDesc.graphicsFormat != 0)
		{
			Resolution = new int2(depthDesc.width, depthDesc.height);
			if (IsDirty(ref depthDesc))
			{
				Reset();
			}
			if (!IsValid())
			{
				Alloc(ref depthDesc);
				return true;
			}
		}
		return false;
	}

	private static int FindMipLevel(ref RenderTextureDescriptor depthDesc)
	{
		int num = 1024;
		float num2 = float.MaxValue;
		int num3 = 0;
		int num4 = depthDesc.width;
		int num5 = depthDesc.height;
		while (num4 > 1 && num3 <= num + 1)
		{
			float num6 = math.abs((float)num4 - 512f);
			if (num6 < num2)
			{
				num = num3;
				num2 = num6;
				depthDesc.width = num4;
				depthDesc.height = num5;
			}
			num4 = num4 + 1 >> 1;
			num5 = num5 + 1 >> 1;
			num3++;
		}
		return math.max(1, num);
	}

	private bool IsDirty(ref RenderTextureDescriptor desc)
	{
		return m_DescKey != Hash128.Compute(ref desc);
	}

	private void Alloc(ref RenderTextureDescriptor desc)
	{
		AllocHistory(m_Id, 1, ref desc, "CullingDepthHistory");
		m_Descriptor = desc;
		m_DescKey = Hash128.Compute(ref desc);
	}

	private bool IsValid()
	{
		return GetTexture() != null;
	}
}
