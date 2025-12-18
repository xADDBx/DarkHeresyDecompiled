using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

internal sealed class MotionVectorsPersistentData
{
	private const int k_EyeCount = 2;

	private readonly Matrix4x4[] m_Projection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_View = new Matrix4x4[2];

	private readonly Matrix4x4[] m_ViewProjection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousProjection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousView = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousViewProjection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousPreviousProjection = new Matrix4x4[2];

	private readonly Matrix4x4[] m_PreviousPreviousView = new Matrix4x4[2];

	private readonly int[] m_LastFrameIndex = new int[2];

	private readonly float[] m_PrevAspectRatio = new float[2];

	private float m_deltaTime;

	private float m_lastDeltaTime;

	private Vector3 m_worldSpaceCameraPos;

	private Vector3 m_previousWorldSpaceCameraPos;

	private Vector3 m_previousPreviousWorldSpaceCameraPos;

	internal int lastFrameIndex => m_LastFrameIndex[0];

	internal Matrix4x4 viewProjection => m_ViewProjection[0];

	internal Matrix4x4 previousViewProjection => m_PreviousViewProjection[0];

	internal Matrix4x4[] viewProjectionStereo => m_ViewProjection;

	internal Matrix4x4[] previousViewProjectionStereo => m_PreviousViewProjection;

	internal Matrix4x4[] projectionStereo => m_Projection;

	internal Matrix4x4[] previousProjectionStereo => m_PreviousProjection;

	internal Matrix4x4[] previousPreviousProjectionStereo => m_PreviousPreviousProjection;

	internal Matrix4x4[] viewStereo => m_View;

	internal Matrix4x4[] previousViewStereo => m_PreviousView;

	internal Matrix4x4[] previousPreviousViewStereo => m_PreviousPreviousView;

	internal float deltaTime => m_deltaTime;

	internal float lastDeltaTime => m_lastDeltaTime;

	internal Vector3 worldSpaceCameraPos => m_worldSpaceCameraPos;

	internal Vector3 previousWorldSpaceCameraPos => m_previousWorldSpaceCameraPos;

	internal Vector3 previousPreviousWorldSpaceCameraPos => m_previousPreviousWorldSpaceCameraPos;

	internal MotionVectorsPersistentData()
	{
		Reset();
	}

	public void Reset()
	{
		for (int i = 0; i < 2; i++)
		{
			m_Projection[i] = Matrix4x4.identity;
			m_View[i] = Matrix4x4.identity;
			m_ViewProjection[i] = Matrix4x4.identity;
			m_PreviousProjection[i] = Matrix4x4.identity;
			m_PreviousView[i] = Matrix4x4.identity;
			m_PreviousViewProjection[i] = Matrix4x4.identity;
			m_PreviousProjection[i] = Matrix4x4.identity;
			m_PreviousView[i] = Matrix4x4.identity;
			m_PreviousViewProjection[i] = Matrix4x4.identity;
			m_LastFrameIndex[i] = -1;
			m_PrevAspectRatio[i] = -1f;
		}
		m_deltaTime = 0f;
		m_lastDeltaTime = 0f;
		m_worldSpaceCameraPos = Vector3.zero;
		m_previousWorldSpaceCameraPos = Vector3.zero;
		m_previousPreviousWorldSpaceCameraPos = Vector3.zero;
	}

	public void Update(WaaaghCameraData cameraData)
	{
		int num = 0;
		bool num2 = 0 == 0 || num == 0;
		int frameCount = Time.frameCount;
		if (num2)
		{
			bool num3 = m_LastFrameIndex[0] == -1;
			float num4 = Time.deltaTime;
			Vector3 position = cameraData.camera.transform.position;
			if (num3)
			{
				m_lastDeltaTime = num4;
				m_deltaTime = num4;
				m_previousPreviousWorldSpaceCameraPos = position;
				m_previousWorldSpaceCameraPos = position;
				m_worldSpaceCameraPos = position;
			}
			m_lastDeltaTime = m_deltaTime;
			m_deltaTime = num4;
			m_previousPreviousWorldSpaceCameraPos = m_previousWorldSpaceCameraPos;
			m_previousWorldSpaceCameraPos = m_worldSpaceCameraPos;
			m_worldSpaceCameraPos = position;
		}
		bool flag = m_PrevAspectRatio[num] != cameraData.aspectRatio;
		if (!(m_LastFrameIndex[num] != frameCount || flag))
		{
			return;
		}
		bool flag2 = m_LastFrameIndex[num] == -1 || flag;
		int num5 = 1;
		for (int i = 0; i < num5; i++)
		{
			int num6 = i + num;
			Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(i), renderIntoTexture: true);
			Matrix4x4 viewMatrix = cameraData.GetViewMatrix(i);
			Matrix4x4 matrix4x = gPUProjectionMatrix * viewMatrix;
			if (flag2)
			{
				m_PreviousPreviousProjection[num6] = gPUProjectionMatrix;
				m_PreviousProjection[num6] = gPUProjectionMatrix;
				m_Projection[num6] = gPUProjectionMatrix;
				m_PreviousPreviousView[num6] = viewMatrix;
				m_PreviousView[num6] = viewMatrix;
				m_View[num6] = viewMatrix;
				m_ViewProjection[num6] = matrix4x;
				m_PreviousViewProjection[num6] = matrix4x;
			}
			m_PreviousPreviousProjection[num6] = m_PreviousProjection[num6];
			m_PreviousProjection[num6] = m_Projection[num6];
			m_Projection[num6] = gPUProjectionMatrix;
			m_PreviousPreviousView[num6] = m_PreviousView[num6];
			m_PreviousView[num6] = m_View[num6];
			m_View[num6] = viewMatrix;
			m_PreviousViewProjection[num6] = m_ViewProjection[num6];
			m_ViewProjection[num6] = matrix4x;
		}
		m_LastFrameIndex[num] = frameCount;
		m_PrevAspectRatio[num] = cameraData.aspectRatio;
	}

	public void SetGlobalMotionMatrices(CommandBuffer cmd)
	{
		int num = 0;
		cmd.SetGlobalMatrix(ShaderPropertyId._PrevViewProjMatrix, previousViewProjectionStereo[num]);
		cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredViewProjMatrix, viewProjectionStereo[num]);
	}
}
