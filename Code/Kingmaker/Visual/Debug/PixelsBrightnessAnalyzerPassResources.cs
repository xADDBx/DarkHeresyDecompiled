using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Debug;

internal sealed class PixelsBrightnessAnalyzerPassResources : IDisposable
{
	public const int CounterElements = 2;

	public const int HistogramBins = 64;

	public readonly ComputeShader Shader;

	public readonly int AnalyzeKernel;

	public readonly int DrawHistogramKernel;

	public readonly LocalKeyword LinearToSrgbConversion;

	public GraphicsBuffer CounterBuffer;

	public GraphicsBuffer HistogramBuffer;

	public readonly uint[] CounterClearData = new uint[2];

	public readonly uint[] HistogramClearData = new uint[64];

	public bool ReadbackPending;

	public bool HistogramReadbackPending;

	public uint HistogramMaxValue = 10000u;

	public int TotalPixels;

	public readonly Action<AsyncGPUReadbackRequest> OnCounterReadback;

	public readonly Action<AsyncGPUReadbackRequest> OnHistogramReadback;

	public PixelsBrightnessAnalyzerPassResources(ComputeShader shader)
	{
		Shader = shader;
		AnalyzeKernel = shader.FindKernel("AnalyzeBrightness");
		DrawHistogramKernel = shader.FindKernel("DrawHistogram");
		LinearToSrgbConversion = new LocalKeyword(shader, "_LINEAR_TO_SRGB_CONVERSION");
		CounterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 2, 4);
		HistogramBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 64, 4);
		OnCounterReadback = HandleCounterReadback;
		OnHistogramReadback = HandleHistogramReadback;
	}

	public void Dispose()
	{
		CounterBuffer?.Dispose();
		CounterBuffer = null;
		HistogramBuffer?.Dispose();
		HistogramBuffer = null;
	}

	private void HandleCounterReadback(AsyncGPUReadbackRequest request)
	{
		ReadbackPending = false;
		if (!request.hasError && request.done)
		{
			NativeArray<uint> data = request.GetData<uint>();
			if (data.Length >= 2 && TotalPixels > 0)
			{
				PixelsBrightnessAnalyzerSettings.DarkPixelPercentage = (float)data[0] / (float)TotalPixels * 100f;
				PixelsBrightnessAnalyzerSettings.BrightPixelPercentage = (float)data[1] / (float)TotalPixels * 100f;
			}
		}
	}

	private void HandleHistogramReadback(AsyncGPUReadbackRequest request)
	{
		HistogramReadbackPending = false;
		if (request.hasError || !request.done)
		{
			return;
		}
		NativeArray<uint> data = request.GetData<uint>();
		uint num = 1u;
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i] > num)
			{
				num = data[i];
			}
		}
		HistogramMaxValue = num;
	}
}
