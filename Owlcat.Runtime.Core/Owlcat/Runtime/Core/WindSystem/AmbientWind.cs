using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.WindSystem;

public class AmbientWind : MonoBehaviour
{
	public static class ShaderPropertyId
	{
		public static readonly int AmbientWindConstantBuffer = Shader.PropertyToID("AmbientWindConstantBuffer");

		public static int _StrengthNoiseWeight = Shader.PropertyToID("_StrengthNoiseWeight");

		public static int _StrengthNoiseContrast = Shader.PropertyToID("_StrengthNoiseContrast");

		public static int _WindVector = Shader.PropertyToID("_WindVector");

		public static int _GlobalWindEnabled = Shader.PropertyToID("_GlobalWindEnabled");

		public static int _CompressedStrengthOctaves = Shader.PropertyToID("_CompressedStrengthOctaves");

		public static int _CompressedShiftOctaves = Shader.PropertyToID("_CompressedShiftOctaves");
	}

	[Serializable]
	public class NoiseOctave
	{
		public float Weight = 1f;

		public float Scale = 0.01f;

		public float MoveSpeed = 1f;

		[NonSerialized]
		public float2 AccumulatedVector;
	}

	private static AmbientWind s_Instance;

	private float2 m_Velocity;

	private Vector4[] m_Strength = new Vector4[2];

	private Vector4[] m_Shift = new Vector4[2];

	private AmbientWindConstantBuffer m_WindCB;

	[SerializeField]
	public float Intensity = 1f;

	[Range(0f, 1f)]
	public float StrengthNoiseWeight = 0.5f;

	[Range(1f, 10f)]
	public float StrengthNoiseContrast = 1f;

	public NoiseOctave StrenghtOctave0 = new NoiseOctave();

	public NoiseOctave StrengthOctave1 = new NoiseOctave();

	public NoiseOctave ShiftOctave0 = new NoiseOctave();

	public NoiseOctave ShiftOctave1 = new NoiseOctave();

	public Vector4[] PackedStrengthOctaves => m_Strength;

	public Vector4[] PackedShiftOctaves => m_Shift;

	public static AmbientWind Instance => s_Instance;

	public float2 Velocity => m_Velocity;

	public IEnumerable<NoiseOctave> EnumerateStrenghtOctaves()
	{
		yield return StrenghtOctave0;
		yield return StrengthOctave1;
	}

	public IEnumerable<NoiseOctave> EnumerateShiftOctaves()
	{
		yield return ShiftOctave0;
		yield return ShiftOctave1;
	}

	private void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
		else if (s_Instance != this)
		{
			Debug.LogWarning("There are several instances of AmbientWind. " + base.name + " will be disabled.");
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
			m_WindCB._GlobalWindEnabled = 0f;
			ConstantBuffer.PushGlobal(in m_WindCB, ShaderPropertyId.AmbientWindConstantBuffer);
		}
	}

	private void Update()
	{
		UpdateNoiseOctaves(m_Strength, m_Shift);
		FillAndPushWindCB();
	}

	public void UpdateNoiseOctaves(Vector4[] strength, Vector4[] shift)
	{
		m_Velocity = ((float3)base.transform.forward).xz * Intensity;
		float num = 0f;
		foreach (NoiseOctave item in EnumerateStrenghtOctaves())
		{
			item.AccumulatedVector += m_Velocity * item.MoveSpeed * item.Scale * Time.deltaTime;
			num += item.Weight;
		}
		int num2 = 0;
		foreach (NoiseOctave item2 in EnumerateStrenghtOctaves())
		{
			strength[num2] = new float4(item2.Weight / num, item2.Scale, item2.AccumulatedVector);
			num2++;
		}
		num = 0f;
		foreach (NoiseOctave item3 in EnumerateShiftOctaves())
		{
			item3.AccumulatedVector += m_Velocity * item3.MoveSpeed * item3.Scale * Time.deltaTime;
			num += item3.Weight;
		}
		num2 = 0;
		foreach (NoiseOctave item4 in EnumerateShiftOctaves())
		{
			shift[num2] = new float4(item4.Weight / num, item4.Scale, item4.AccumulatedVector);
			num2++;
		}
	}

	private void FillAndPushWindCB()
	{
		m_WindCB._GlobalWindEnabled = 0f;
		if (Instance != null)
		{
			m_WindCB._GlobalWindEnabled = 1f;
			m_WindCB._WindVector = Instance.Velocity;
			m_WindCB._StrengthNoiseWeight = Instance.StrengthNoiseWeight;
			m_WindCB._StrengthNoiseContrast = Instance.StrengthNoiseContrast;
			for (int i = 0; i < 2; i++)
			{
				m_WindCB.SetStrengthOctave(i, Instance.PackedStrengthOctaves[i]);
			}
			for (int j = 0; j < 2; j++)
			{
				m_WindCB.SetShiftOctave(j, Instance.PackedShiftOctaves[j]);
			}
		}
		ConstantBuffer.PushGlobal(in m_WindCB, ShaderPropertyId.AmbientWindConstantBuffer);
	}
}
