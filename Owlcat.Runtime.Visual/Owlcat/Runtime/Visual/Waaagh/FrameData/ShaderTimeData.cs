using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct ShaderTimeData
{
	public Vector4 Time;

	public Vector4 SinTime;

	public Vector4 CosTime;

	public Vector4 DeltaTime;

	public Vector4 TimeParameters;

	public Vector4 UnscaledTime;

	public Vector4 UnscaledTimeParameters;

	public ShaderTimeData(in TimeData timeData)
	{
		float time = timeData.Time;
		float deltaTime = timeData.DeltaTime;
		float smoothDeltaTime = timeData.SmoothDeltaTime;
		float unscaledTime = timeData.UnscaledTime;
		float f = time / 8f;
		float f2 = time / 4f;
		float f3 = time / 2f;
		Vector4 time2 = time * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 sinTime = new Vector4(Mathf.Sin(f), Mathf.Sin(f2), Mathf.Sin(f3), Mathf.Sin(time));
		Vector4 cosTime = new Vector4(Mathf.Cos(f), Mathf.Cos(f2), Mathf.Cos(f3), Mathf.Cos(time));
		Vector4 deltaTime2 = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
		Vector4 timeParameters = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0f);
		Vector4 unscaledTime2 = unscaledTime * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 unscaledTimeParameters = new Vector4(unscaledTime, Mathf.Sin(unscaledTime), Mathf.Cos(unscaledTime), 0f);
		Time = time2;
		SinTime = sinTime;
		CosTime = cosTime;
		DeltaTime = deltaTime2;
		TimeParameters = timeParameters;
		UnscaledTime = unscaledTime2;
		UnscaledTimeParameters = unscaledTimeParameters;
	}

	internal void PushGlobal(UnsafeCommandBuffer cmd)
	{
		cmd.SetGlobalVector(ShaderPropertyId._Time, Time);
		cmd.SetGlobalVector(ShaderPropertyId._SinTime, SinTime);
		cmd.SetGlobalVector(ShaderPropertyId._CosTime, CosTime);
		cmd.SetGlobalVector(ShaderPropertyId.unity_DeltaTime, DeltaTime);
		cmd.SetGlobalVector(ShaderPropertyId._TimeParameters, TimeParameters);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTime, UnscaledTime);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTimeParameters, UnscaledTimeParameters);
	}

	internal void PushGlobal(RasterCommandBuffer cmd)
	{
		cmd.SetGlobalVector(ShaderPropertyId._Time, Time);
		cmd.SetGlobalVector(ShaderPropertyId._SinTime, SinTime);
		cmd.SetGlobalVector(ShaderPropertyId._CosTime, CosTime);
		cmd.SetGlobalVector(ShaderPropertyId.unity_DeltaTime, DeltaTime);
		cmd.SetGlobalVector(ShaderPropertyId._TimeParameters, TimeParameters);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTime, UnscaledTime);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTimeParameters, UnscaledTimeParameters);
	}
}
