using System;
using Owlcat.Runtime.Core.WindSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class VFXWindSystem : IDisposable
{
	private WindProfile m_Profile;

	private WindParams m_CurrentParams = WindParams.Zero;

	private InclemencyType m_WeatherInclemency;

	private WindParams m_PreviousParamsFromWeather = WindParams.Zero;

	private WindParams m_NewParamsFromWeather = WindParams.Zero;

	private WindParams m_PreviousParamsFromWind = WindParams.Zero;

	private WindParams m_NewParamsFromWind = WindParams.Zero;

	private NativeArray<float4> m_CompressedStrengthOctaves;

	private NativeArray<float4> m_CompressedShiftOctaves;

	private float m_CurrentIntensity;

	private Vector2 m_CurrentDirection;

	private AmbientWindConstantBuffer m_WindCB;

	public float CurrentIntensity => m_CurrentIntensity;

	public Vector2 CurrentDirection => m_CurrentDirection;

	public VFXWindSystem(WindProfile profile)
	{
		m_Profile = profile;
		CreateNativeArrays();
	}

	public void SetNewWeatherInclemency(InclemencyType inclemency)
	{
		m_PreviousParamsFromWeather = m_NewParamsFromWeather;
		m_NewParamsFromWeather = WindParams.Sample(m_Profile, inclemency);
		m_WeatherInclemency = inclemency;
	}

	public void SetNewWindInclemency(InclemencyType inclemency)
	{
		m_PreviousParamsFromWind = m_NewParamsFromWind;
		m_NewParamsFromWind = WindParams.Sample(m_Profile, inclemency);
	}

	public void Update(float inclemencyChangePercentageFromWeather, float inclemencyChangePercentageFromWind)
	{
		WindParams b = WindParams.Lerp(m_PreviousParamsFromWeather, m_NewParamsFromWeather, inclemencyChangePercentageFromWeather);
		WindParams a = WindParams.Lerp(m_PreviousParamsFromWind, m_NewParamsFromWind, inclemencyChangePercentageFromWind);
		m_CurrentParams = WindParams.Lerp(a, b, m_Profile.WindLerpValues[m_WeatherInclemency]);
		m_CurrentIntensity = m_CurrentParams.Intensity;
		m_CurrentDirection = m_CurrentParams.Direction;
		FillAndPushWindCB();
	}

	public void DebugUpdate(float debugWindIntensity, Vector2 debugWindDirection)
	{
		m_CurrentIntensity = debugWindIntensity;
		if (debugWindDirection.sqrMagnitude > 0.01f)
		{
			m_CurrentDirection = debugWindDirection.normalized;
		}
		else
		{
			m_CurrentDirection = Vector2.right;
		}
		m_CurrentParams = new WindParams(m_CurrentDirection, m_CurrentIntensity);
		FillAndPushWindCB();
	}

	public void Dispose()
	{
		DisposeNativeArrays();
		DisableWind();
	}

	private void DisableWind()
	{
		m_WindCB._GlobalWindEnabled = 0f;
		ConstantBuffer.PushGlobal(in m_WindCB, AmbientWind.ShaderPropertyId.AmbientWindConstantBuffer);
	}

	private void FillAndPushWindCB()
	{
		m_WindCB._GlobalWindEnabled = 1f;
		float2 @float = m_CurrentParams.GetFloat2();
		m_WindCB._WindVector = @float;
		m_WindCB._StrengthNoiseWeight = m_Profile.StrengthNoiseWeight;
		m_WindCB._StrengthNoiseContrast = m_Profile.StrengthNoiseContrast;
		m_Profile.StrengthNoiseSettings.Tick(@float, Time.deltaTime);
		m_Profile.ShiftNoiseSettings.Tick(@float, Time.deltaTime);
		float4[] weightedCompressed = m_Profile.StrengthNoiseSettings.GetWeightedCompressed();
		float4[] compressed = m_Profile.ShiftNoiseSettings.GetCompressed();
		for (int i = 0; i < weightedCompressed.Length; i++)
		{
			m_WindCB.SetStrengthOctave(i, weightedCompressed[i]);
		}
		for (int j = 0; j < compressed.Length; j++)
		{
			m_WindCB.SetShiftOctave(j, compressed[j]);
		}
		ConstantBuffer.PushGlobal(in m_WindCB, AmbientWind.ShaderPropertyId.AmbientWindConstantBuffer);
	}

	private void CreateNativeArrays()
	{
		DisposeNativeArrays();
		m_CompressedStrengthOctaves = new NativeArray<float4>(2, Allocator.Persistent);
		m_CompressedShiftOctaves = new NativeArray<float4>(2, Allocator.Persistent);
	}

	private void DisposeNativeArrays()
	{
		if (m_CompressedStrengthOctaves.IsCreated)
		{
			m_CompressedStrengthOctaves.Dispose();
		}
		if (m_CompressedShiftOctaves.IsCreated)
		{
			m_CompressedShiftOctaves.Dispose();
		}
	}
}
