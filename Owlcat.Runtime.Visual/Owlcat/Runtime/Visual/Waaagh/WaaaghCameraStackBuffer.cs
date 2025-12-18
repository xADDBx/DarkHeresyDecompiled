using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghCameraStackBuffer : IDisposable
{
	public RTHandle ColorBuffer;

	public RTHandle DepthBuffer;

	internal int UnusedFramesCount;

	internal WaaaghCameraStackBuffer(WaaaghCameraData cameraData)
	{
		Alloc(cameraData);
	}

	public void Dispose()
	{
		RTHandles.Release(ColorBuffer);
		RTHandles.Release(DepthBuffer);
	}

	internal void Update(WaaaghCameraData cameraData)
	{
		UnusedFramesCount = 0;
		int2 @int = new int2(cameraData.pixelWidth, cameraData.pixelHeight);
		if (ColorBuffer.rt.width != @int.x || ColorBuffer.rt.height != @int.y || ColorBuffer.rt.graphicsFormat != cameraData.cameraTargetDescriptor.graphicsFormat)
		{
			Dispose();
			Alloc(cameraData);
		}
	}

	private void Alloc(WaaaghCameraData cameraData)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.depthBufferBits = 0;
		descriptor.enableRandomWrite = true;
		RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
		descriptor2.graphicsFormat = GraphicsFormat.D24_UNorm_S8_UInt;
		descriptor2.depthBufferBits = 32;
		int2 @int = new int2(cameraData.pixelWidth, cameraData.pixelHeight);
		descriptor.width = @int.x;
		descriptor.height = @int.y;
		ColorBuffer = RTHandles.Alloc(in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "CameraColorRT");
		descriptor2.width = @int.x;
		descriptor2.height = @int.y;
		DepthBuffer = RTHandles.Alloc(in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "CameraDepthRT");
	}
}
