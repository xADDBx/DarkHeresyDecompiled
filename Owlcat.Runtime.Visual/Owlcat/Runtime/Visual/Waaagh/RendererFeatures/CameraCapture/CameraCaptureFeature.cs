using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraCapture;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/CameraCaptureFeature")]
public sealed class CameraCaptureFeature : ScriptableRendererFeature
{
	[Flags]
	public enum CaptureOptions
	{
		None = 0,
		ExcludeScreenSpaceOverlayUI = 1
	}

	public enum CaptureSize
	{
		ScreenSizeDownscale,
		ExactSize
	}

	public struct CaptureRequest
	{
		public Camera Camera;

		public CaptureOptions Options;

		public CaptureSize CaptureSize;

		public uint OutputDownscale;

		public uint OutputExactWidth;

		public uint OutputExactHeight;

		public CaptureCallback Callback;

		public object State;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct PreUpdate
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct PostLateUpdate
	{
	}

	public delegate void CaptureCallback(object state, RenderTexture texture);

	private sealed class CapturePass : ScriptableRenderPass
	{
		private sealed class PassData
		{
			public TextureHandle Source;

			public readonly List<BlitInfo> Blits = new List<BlitInfo>();
		}

		private struct BlitInfo
		{
			public RenderTexture Target;

			public Rect Viewport;
		}

		public readonly List<CaptureRequest> Requests = new List<CaptureRequest>();

		public readonly List<RenderTexture> Results = new List<RenderTexture>();

		public override string Name => "Capture";

		public CapturePass(RenderPassEvent evt)
			: base(evt)
		{
		}

		public override void RecordRenderGraph(ContextContainer frameData)
		{
			if (Requests.Count == 0)
			{
				return;
			}
			WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
			RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
			Results.Clear();
			PassData passData;
			RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\CameraCapture\\CameraCaptureFeature.cs", 299);
			try
			{
				passData.Source = waaaghResourceData.CameraColorBuffer;
				passData.Blits.Clear();
				TextureDesc srcDesc = renderGraph.GetTextureDesc(waaaghResourceData.CameraColorBuffer);
				for (int i = 0; i < Requests.Count; i++)
				{
					CaptureRequest request = Requests[i];
					BlitInfo item = CreateBlitInfo(in srcDesc, in request);
					passData.Blits.Add(item);
					Results.Add(item.Target);
				}
				TextureHandle input = waaaghResourceData.CameraColorBuffer;
				renderGraphBuilder.ReadTexture(in input);
				renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
				{
					foreach (BlitInfo blit in data.Blits)
					{
						context.cmd.SetRenderTarget(blit.Target);
						Rect viewport = blit.Viewport;
						if (viewport.width > 0f)
						{
							context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
							context.cmd.SetViewport(blit.Viewport);
						}
						Blitter.BlitTexture2D(context.cmd, data.Source, new Vector4(1f, 1f, 0f, 0f), 0f, bilinear: true);
					}
				});
			}
			finally
			{
				((IDisposable)renderGraphBuilder).Dispose();
			}
		}

		private static BlitInfo CreateBlitInfo(in TextureDesc srcDesc, in CaptureRequest request)
		{
			switch (request.CaptureSize)
			{
			case CaptureSize.ScreenSizeDownscale:
			{
				int width = srcDesc.width >> (int)request.OutputDownscale;
				int height = srcDesc.height >> (int)request.OutputDownscale;
				RenderTexture target2 = new RenderTexture(width, height, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.None, 1);
				BlitInfo result = default(BlitInfo);
				result.Target = target2;
				result.Viewport = default(Rect);
				return result;
			}
			case CaptureSize.ExactSize:
			{
				int outputExactWidth = (int)request.OutputExactWidth;
				int outputExactHeight = (int)request.OutputExactHeight;
				RenderTexture target = new RenderTexture(outputExactWidth, outputExactHeight, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.None, 1);
				float num = (float)srcDesc.width / (float)srcDesc.height;
				float b = (float)outputExactWidth / (float)outputExactHeight;
				BlitInfo result;
				if (Mathf.Approximately(num, b))
				{
					result = default(BlitInfo);
					result.Target = target;
					result.Viewport = default(Rect);
					return result;
				}
				int num2 = outputExactHeight;
				int num3 = Mathf.RoundToInt((float)num2 * num);
				int num4 = (outputExactWidth - num3) / 2;
				Rect viewport = new Rect(num4, 0f, num3, num2);
				result = default(BlitInfo);
				result.Target = target;
				result.Viewport = viewport;
				return result;
			}
			default:
				throw new ArgumentOutOfRangeException("CaptureSize");
			}
		}
	}

	private static readonly List<CaptureRequest> s_PendingRequests = new List<CaptureRequest>();

	private static readonly List<CaptureRequest> s_ProcessingRequests = new List<CaptureRequest>();

	private static readonly CapturePass s_CaptureBeforeOverlayPass = new CapturePass(RenderPassEvent.BeforeRenderingOverlayUI);

	private static readonly CapturePass s_CaptureAfterOverlayPass = new CapturePass(RenderPassEvent.AfterRenderingOverlayUI);

	private static bool s_Initialized;

	public static void Capture(CaptureRequest request)
	{
		if (request.Callback != null)
		{
			s_PendingRequests.Add(request);
			EnsureInitialized();
		}
	}

	private static void EnsureInitialized()
	{
		if (!s_Initialized)
		{
			PlayerLoopUtility.RegisterUpdateDelegate(typeof(UnityEngine.PlayerLoop.PreUpdate), typeof(PreUpdate), OnPreUpdate);
			PlayerLoopUtility.RegisterUpdateDelegate(typeof(UnityEngine.PlayerLoop.PostLateUpdate), typeof(PostLateUpdate), OnPostLateUpdate);
			s_Initialized = true;
		}
	}

	private static void OnPreUpdate()
	{
		FinishProcessing();
	}

	private static void OnPostLateUpdate()
	{
		StartProcessing();
	}

	private static void StartProcessing()
	{
		if (s_PendingRequests.Count != 0)
		{
			s_ProcessingRequests.Clear();
			s_ProcessingRequests.AddRange(s_PendingRequests);
			s_PendingRequests.Clear();
			s_CaptureBeforeOverlayPass.Requests.Clear();
			s_CaptureBeforeOverlayPass.Results.Clear();
			s_CaptureAfterOverlayPass.Requests.Clear();
			s_CaptureAfterOverlayPass.Results.Clear();
		}
	}

	private static void FinishProcessing()
	{
		if (s_ProcessingRequests.Count <= 0 && s_CaptureBeforeOverlayPass.Requests.Count <= 0 && s_CaptureAfterOverlayPass.Requests.Count <= 0)
		{
			return;
		}
		try
		{
			NotifySuccess(s_CaptureBeforeOverlayPass);
			NotifySuccess(s_CaptureAfterOverlayPass);
			NotifyFailure(s_ProcessingRequests);
		}
		finally
		{
			s_ProcessingRequests.Clear();
			s_CaptureBeforeOverlayPass.Requests.Clear();
			s_CaptureBeforeOverlayPass.Results.Clear();
			s_CaptureAfterOverlayPass.Requests.Clear();
			s_CaptureAfterOverlayPass.Results.Clear();
		}
		static void NotifyFailure(List<CaptureRequest> requests)
		{
			foreach (CaptureRequest request in requests)
			{
				request.Callback?.Invoke(request.State, null);
			}
		}
		static void NotifySuccess(CapturePass pass)
		{
			for (int i = 0; i < pass.Requests.Count; i++)
			{
				CaptureRequest captureRequest = pass.Requests[i];
				RenderTexture texture = pass.Results[i];
				captureRequest.Callback?.Invoke(captureRequest.State, texture);
			}
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (s_ProcessingRequests.Count == 0)
		{
			return;
		}
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		bool flag = waaaghCameraData.CameraResolveTargetBufferType == CameraResolveTargetType.Backbuffer;
		int num = 0;
		while (num < s_ProcessingRequests.Count)
		{
			CaptureRequest item = s_ProcessingRequests[num];
			if ((item.Camera != null) ? (item.Camera == waaaghCameraData.camera) : (waaaghCameraData.cameraType == CameraType.Game && flag))
			{
				if ((item.Options & CaptureOptions.ExcludeScreenSpaceOverlayUI) != 0)
				{
					s_CaptureBeforeOverlayPass.Requests.Add(item);
				}
				else
				{
					s_CaptureAfterOverlayPass.Requests.Add(item);
				}
				s_ProcessingRequests.RemoveAtSwapBack(num);
			}
			else
			{
				num++;
			}
		}
		if (s_CaptureBeforeOverlayPass.Requests.Count > 0)
		{
			renderer.EnqueuePass(s_CaptureBeforeOverlayPass);
		}
		if (s_CaptureAfterOverlayPass.Requests.Count > 0)
		{
			renderer.EnqueuePass(s_CaptureAfterOverlayPass);
		}
	}

	public override void Create()
	{
	}
}
