using Owlcat.Runtime.Visual;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Kingmaker.Visual.Debug;

internal static class PixelsBrightnessAnalyzerPass
{
	private class AnalyzePassData
	{
		public PixelsBrightnessAnalyzerPassResources Resources;

		public TextureHandle Source;

		public TextureHandle Result;

		public BufferHandle Counter;

		public BufferHandle Histogram;

		public int Width;

		public int Height;

		public float DarkThreshold;

		public float BrightThreshold;

		public Color DarkColor;

		public Color BrightColor;

		public bool IsCameraColorSrgb;

		public bool HighlightEnabled;

		public int BrightnessMode;

		public bool RequestCounterReadback;

		public bool RequestHistogramReadback;
	}

	private class HistogramPassData
	{
		public PixelsBrightnessAnalyzerPassResources Resources;

		public TextureHandle Target;

		public BufferHandle Histogram;

		public int Width;

		public int Height;

		public int HistW;

		public int HistH;

		public int OffsetX;

		public int OffsetY;

		public uint HistogramMaxValue;

		public float DarkThreshold;

		public float BrightThreshold;

		public Color DarkColor;

		public Color BrightColor;
	}

	private class BlitPassData
	{
		public TextureHandle Source;

		public TextureHandle Destination;
	}

	private static readonly int _SourceTexture = Shader.PropertyToID("_SourceTexture");

	private static readonly int _ResultTexture = Shader.PropertyToID("_ResultTexture");

	private static readonly int _CounterBuffer = Shader.PropertyToID("_CounterBuffer");

	private static readonly int _HistogramBuffer = Shader.PropertyToID("_HistogramBuffer");

	private static readonly int _DarkThreshold = Shader.PropertyToID("_DarkThreshold");

	private static readonly int _BrightThreshold = Shader.PropertyToID("_BrightThreshold");

	private static readonly int _DarkColor = Shader.PropertyToID("_DarkColor");

	private static readonly int _BrightColor = Shader.PropertyToID("_BrightColor");

	private static readonly int _ScreenSize = Shader.PropertyToID("_ScreenSize");

	private static readonly int _HighlightEnabled = Shader.PropertyToID("_HighlightEnabled");

	private static readonly int _HistogramRect = Shader.PropertyToID("_HistogramRect");

	private static readonly int _HistogramOffset = Shader.PropertyToID("_HistogramOffset");

	private static readonly int _HistogramMaxCount = Shader.PropertyToID("_HistogramMaxCount");

	private static readonly int _HistogramBorder = Shader.PropertyToID("_HistogramBorder");

	private static readonly int _HistogramPadding = Shader.PropertyToID("_HistogramPadding");

	private static readonly int _BrightnessMode = Shader.PropertyToID("_BrightnessMode");

	private const int ThreadGroupSize = 8;

	public static void Record(in RecordContext context, PixelsBrightnessAnalyzerPassResources res, bool beforePostProcessing)
	{
		WaaaghCameraData cameraData = context.CameraData;
		if (!cameraData.isGameCamera)
		{
			return;
		}
		RenderGraph renderGraph = context.RenderGraph;
		CameraStackTargets cameraStackTargets = context.FrameResources.CameraStackTargets;
		TextureHandle input = (beforePostProcessing ? cameraStackTargets.Color : cameraStackTargets.CurrentPostProcessSource);
		TextureHandle input2 = input;
		int width = cameraData.cameraTargetDescriptor.width;
		int height = cameraData.cameraTargetDescriptor.height;
		res.TotalPixels = width * height;
		bool isCameraColorSrgb = GraphicsFormatUtility.IsSRGBFormat(cameraData.cameraTargetDescriptor.graphicsFormat);
		bool showHighlight = PixelsBrightnessAnalyzerSettings.ShowHighlight;
		bool showHistogram = PixelsBrightnessAnalyzerSettings.ShowHistogram;
		bool flag = showHighlight || showHistogram;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("PixelsBrightnessResult", cameraData.cameraTargetDescriptor);
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		TextureHandle input3 = renderGraph.CreateTexture(in desc);
		BufferHandle input4 = renderGraph.ImportBuffer(res.CounterBuffer);
		BufferHandle input5 = renderGraph.ImportBuffer(res.HistogramBuffer);
		float darkThreshold = PixelsBrightnessAnalyzerSettings.DarkThreshold;
		float brightThreshold = PixelsBrightnessAnalyzerSettings.BrightThreshold;
		bool flag2 = !res.ReadbackPending;
		bool flag3 = showHistogram && !res.HistogramReadbackPending;
		if (flag2)
		{
			res.ReadbackPending = true;
		}
		if (flag3)
		{
			res.HistogramReadbackPending = true;
		}
		AnalyzePassData passData;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<AnalyzePassData>("PixelsBrightness Analyze", out passData, "E:\\wh-workspace\\wh2-production\\WH2\\Assets\\Code\\View\\Visual\\Debug\\PixelsBrightnessAnalyzer\\PixelsBrightnessAnalyzerPass.cs", 186))
		{
			passData.Resources = res;
			passData.Source = input2;
			passData.Result = input3;
			passData.Counter = input4;
			passData.Histogram = input5;
			passData.Width = width;
			passData.Height = height;
			passData.DarkThreshold = darkThreshold;
			passData.BrightThreshold = brightThreshold;
			passData.DarkColor = PixelsBrightnessAnalyzerSettings.DarkColor;
			passData.BrightColor = PixelsBrightnessAnalyzerSettings.BrightColor;
			passData.IsCameraColorSrgb = isCameraColorSrgb;
			passData.HighlightEnabled = showHighlight;
			passData.BrightnessMode = (int)PixelsBrightnessAnalyzerSettings.Mode;
			passData.RequestCounterReadback = flag2;
			passData.RequestHistogramReadback = flag3;
			unsafeRenderGraphBuilder.UseTexture(in input2);
			unsafeRenderGraphBuilder.UseTexture(in input3, AccessFlags.WriteAll);
			unsafeRenderGraphBuilder.UseBuffer(in input4, AccessFlags.WriteAll);
			unsafeRenderGraphBuilder.UseBuffer(in input5, AccessFlags.WriteAll);
			unsafeRenderGraphBuilder.AllowPassCulling(value: false);
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(AnalyzePassData data, UnsafeGraphContext context)
			{
				CommandBuffer nativeCommandBuffer2 = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
				PixelsBrightnessAnalyzerPassResources resources2 = data.Resources;
				nativeCommandBuffer2.SetBufferData(resources2.CounterBuffer, resources2.CounterClearData);
				nativeCommandBuffer2.SetBufferData(resources2.HistogramBuffer, resources2.HistogramClearData);
				nativeCommandBuffer2.SetKeyword(resources2.Shader, in resources2.LinearToSrgbConversion, data.IsCameraColorSrgb);
				nativeCommandBuffer2.SetComputeTextureParam(resources2.Shader, resources2.AnalyzeKernel, _SourceTexture, data.Source);
				nativeCommandBuffer2.SetComputeTextureParam(resources2.Shader, resources2.AnalyzeKernel, _ResultTexture, data.Result);
				nativeCommandBuffer2.SetComputeBufferParam(resources2.Shader, resources2.AnalyzeKernel, _CounterBuffer, data.Counter);
				nativeCommandBuffer2.SetComputeBufferParam(resources2.Shader, resources2.AnalyzeKernel, _HistogramBuffer, data.Histogram);
				nativeCommandBuffer2.SetComputeFloatParam(resources2.Shader, _DarkThreshold, data.DarkThreshold);
				nativeCommandBuffer2.SetComputeFloatParam(resources2.Shader, _BrightThreshold, data.BrightThreshold);
				nativeCommandBuffer2.SetComputeVectorParam(resources2.Shader, _DarkColor, data.DarkColor);
				nativeCommandBuffer2.SetComputeVectorParam(resources2.Shader, _BrightColor, data.BrightColor);
				nativeCommandBuffer2.SetComputeIntParams(resources2.Shader, _ScreenSize, data.Width, data.Height);
				nativeCommandBuffer2.SetComputeIntParam(resources2.Shader, _HighlightEnabled, data.HighlightEnabled ? 1 : 0);
				nativeCommandBuffer2.SetComputeIntParam(resources2.Shader, _BrightnessMode, data.BrightnessMode);
				int threadGroupsX2 = Mathf.CeilToInt((float)data.Width / 8f);
				int threadGroupsY2 = Mathf.CeilToInt((float)data.Height / 8f);
				nativeCommandBuffer2.DispatchCompute(resources2.Shader, resources2.AnalyzeKernel, threadGroupsX2, threadGroupsY2, 1);
				if (data.RequestCounterReadback)
				{
					nativeCommandBuffer2.RequestAsyncReadback(resources2.CounterBuffer, 8, 0, resources2.OnCounterReadback);
				}
				if (data.RequestHistogramReadback)
				{
					nativeCommandBuffer2.RequestAsyncReadback(resources2.HistogramBuffer, 256, 0, resources2.OnHistogramReadback);
				}
			});
		}
		if (showHistogram)
		{
			HistogramPassData passData2;
			using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder2 = renderGraph.AddUnsafePass<HistogramPassData>("PixelsBrightness Histogram", out passData2, "E:\\wh-workspace\\wh2-production\\WH2\\Assets\\Code\\View\\Visual\\Debug\\PixelsBrightnessAnalyzer\\PixelsBrightnessAnalyzerPass.cs", 263);
			int histW = width / 5;
			int histH = height / 6;
			int offsetX = 10;
			int offsetY = 10;
			passData2.Resources = res;
			passData2.Target = input3;
			passData2.Histogram = input5;
			passData2.Width = width;
			passData2.Height = height;
			passData2.HistW = histW;
			passData2.HistH = histH;
			passData2.OffsetX = offsetX;
			passData2.OffsetY = offsetY;
			passData2.HistogramMaxValue = res.HistogramMaxValue;
			passData2.DarkThreshold = darkThreshold;
			passData2.BrightThreshold = brightThreshold;
			passData2.DarkColor = PixelsBrightnessAnalyzerSettings.DarkColor;
			passData2.BrightColor = PixelsBrightnessAnalyzerSettings.BrightColor;
			unsafeRenderGraphBuilder2.UseTexture(in input3, AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder2.UseBuffer(in input5);
			unsafeRenderGraphBuilder2.AllowPassCulling(value: false);
			unsafeRenderGraphBuilder2.SetRenderFunc(delegate(HistogramPassData data, UnsafeGraphContext context)
			{
				CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
				PixelsBrightnessAnalyzerPassResources resources = data.Resources;
				nativeCommandBuffer.SetComputeTextureParam(resources.Shader, resources.DrawHistogramKernel, _ResultTexture, data.Target);
				nativeCommandBuffer.SetComputeBufferParam(resources.Shader, resources.DrawHistogramKernel, _HistogramBuffer, data.Histogram);
				nativeCommandBuffer.SetComputeIntParams(resources.Shader, _ScreenSize, data.Width, data.Height);
				nativeCommandBuffer.SetComputeIntParams(resources.Shader, _HistogramRect, data.HistW, data.HistH);
				nativeCommandBuffer.SetComputeIntParams(resources.Shader, _HistogramOffset, data.OffsetX, data.OffsetY);
				nativeCommandBuffer.SetComputeIntParam(resources.Shader, _HistogramMaxCount, (int)data.HistogramMaxValue);
				nativeCommandBuffer.SetComputeIntParam(resources.Shader, _HistogramBorder, 2);
				nativeCommandBuffer.SetComputeIntParam(resources.Shader, _HistogramPadding, 4);
				nativeCommandBuffer.SetComputeFloatParam(resources.Shader, _DarkThreshold, data.DarkThreshold);
				nativeCommandBuffer.SetComputeFloatParam(resources.Shader, _BrightThreshold, data.BrightThreshold);
				nativeCommandBuffer.SetComputeVectorParam(resources.Shader, _DarkColor, data.DarkColor);
				nativeCommandBuffer.SetComputeVectorParam(resources.Shader, _BrightColor, data.BrightColor);
				int threadGroupsX = Mathf.CeilToInt((float)data.HistW / 8f);
				int threadGroupsY = Mathf.CeilToInt((float)data.HistH / 8f);
				nativeCommandBuffer.DispatchCompute(resources.Shader, resources.DrawHistogramKernel, threadGroupsX, threadGroupsY, 1);
			});
		}
		if (!flag)
		{
			return;
		}
		BlitPassData passData3;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder3 = renderGraph.AddUnsafePass<BlitPassData>("PixelsBrightness Blit", out passData3, "E:\\wh-workspace\\wh2-production\\WH2\\Assets\\Code\\View\\Visual\\Debug\\PixelsBrightnessAnalyzer\\PixelsBrightnessAnalyzerPass.cs", 318);
		passData3.Source = input3;
		passData3.Destination = input;
		unsafeRenderGraphBuilder3.UseTexture(in input3);
		unsafeRenderGraphBuilder3.UseTexture(in input, AccessFlags.Write);
		unsafeRenderGraphBuilder3.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder3.SetRenderFunc(delegate(BlitPassData data, UnsafeGraphContext context)
		{
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(data.Source, data.Destination);
		});
	}
}
