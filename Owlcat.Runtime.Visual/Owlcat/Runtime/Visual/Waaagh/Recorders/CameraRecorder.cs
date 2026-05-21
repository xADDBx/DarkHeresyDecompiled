using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class CameraRecorder
{
	private class PassData
	{
		public WaaaghCameraData CameraData;

		public Matrix4x4 ViewMatrix;

		public Matrix4x4 ProjectionMatrix;

		public Matrix4x4 NonJitteredGpuProjectionMatrix;

		public Matrix4x4 NonJitteredProjectionMatrix;

		public Matrix4x4 NonJitteredViewProjectionMatrix;

		public Matrix4x4 WorldToCameraMatrix;

		public Matrix4x4 CameraToWorldMatrix;

		public Matrix4x4 InverseViewMatrix;

		public Matrix4x4 InverseProjectionMatrix;

		public Matrix4x4 InverseViewProjectionMatrix;

		public Plane[] CameraPlanes = new Plane[6];

		public Vector4[] CameraVectorPlanes = new Vector4[6];

		public Vector4 BillboardNormal;

		public Vector4 BillboardTangent;

		public Vector4 BillboardCameraParams;

		public Vector4 ProjectionParams;

		public Vector3 WorldSpaceCameraPos;

		public Vector4 ScreenParams;

		public Vector4 ScaledScreenParams;

		public Vector4 ZBufferParams;

		public Vector4 OrthoParams;

		public Vector4 ScreenSize;

		public Vector4 GlobalMipBias;

		public ShaderTimeData ShaderTimeData;
	}

	private class ResetJitterPassData
	{
		public Matrix4x4 ViewMatrix;

		public Matrix4x4 ProjectionMatrix;

		public Matrix4x4 InverseProjectionMatrix;

		public Matrix4x4 InverseViewProjectionMatrix;
	}

	private class VFXPrepareCameraPassData
	{
		public Camera Camera;

		public CullingResults CullingResults;
	}

	public static void SetupCamera(in RecordContext context, bool jitter)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<PassData>("Setup Camera", out passData, WaaaghProfileId.SetupCamera.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\CameraRecorder.cs", 56);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		passData.CameraData = context.CameraData;
		passData.ShaderTimeData = context.RenderingData.ShaderTimeData;
		SetupMatrices(passData, jitter);
		SetupParameters(passData);
		rasterRenderGraphBuilder.SetRenderFunc<PassData>(Execute);
	}

	private static void SetupMatrices(PassData passData, bool jitter)
	{
		if (passData.CameraData.renderType == CameraRenderType.Base)
		{
			PrepareCameraMatrices(passData, passData.CameraData, setInverseMatrices: true, jitter);
			return;
		}
		PrepareCameraMatrices(passData, passData.CameraData, setInverseMatrices: true, jitter);
		PrepareClippingPlanes(passData, passData.CameraData, jitter);
		PrepareBillboard(passData, passData.CameraData);
	}

	private static void PrepareCameraMatrices(PassData data, WaaaghCameraData cameraData, bool setInverseMatrices, bool jitter)
	{
		data.ViewMatrix = cameraData.GetViewMatrix();
		data.ProjectionMatrix = cameraData.GetProjectionMatrix();
		data.NonJitteredProjectionMatrix = cameraData.GetProjectionMatrixNoJitter();
		Matrix4x4 m = cameraData.GetGPUProjectionMatrix();
		data.NonJitteredGpuProjectionMatrix = cameraData.GetGPUProjectionMatrixNoJitter();
		data.NonJitteredViewProjectionMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(data.NonJitteredGpuProjectionMatrix, data.ViewMatrix, cameraData.camera.orthographic);
		if (!jitter)
		{
			m = data.NonJitteredGpuProjectionMatrix;
			data.ProjectionMatrix = data.NonJitteredProjectionMatrix;
		}
		if (setInverseMatrices)
		{
			data.InverseViewMatrix = Matrix4x4.Inverse(data.ViewMatrix);
			data.InverseProjectionMatrix = Matrix4x4.Inverse(m);
			data.InverseViewProjectionMatrix = data.InverseViewMatrix * data.InverseProjectionMatrix;
			data.WorldToCameraMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * data.ViewMatrix;
			data.CameraToWorldMatrix = data.WorldToCameraMatrix.inverse;
		}
	}

	private static void PrepareClippingPlanes(PassData data, WaaaghCameraData cameraData, bool jitter)
	{
		Matrix4x4 projMatrix = ((!jitter) ? cameraData.GetGPUProjectionMatrixNoJitter() : cameraData.GetGPUProjectionMatrix());
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		GeometryUtility.CalculateFrustumPlanes(CoreMatrixUtils.MultiplyProjectionMatrix(projMatrix, viewMatrix, cameraData.camera.orthographic), data.CameraPlanes);
		for (int i = 0; i < data.CameraPlanes.Length; i++)
		{
			data.CameraVectorPlanes[i] = new Vector4(data.CameraPlanes[i].normal.x, data.CameraPlanes[i].normal.y, data.CameraPlanes[i].normal.z, data.CameraPlanes[i].distance);
		}
	}

	private static void PrepareBillboard(PassData data, WaaaghCameraData cameraData)
	{
		Matrix4x4 worldToCameraMatrix = cameraData.GetViewMatrix();
		Vector3 worldSpaceCameraPos = cameraData.worldSpaceCameraPos;
		CalculateBillboardProperties(in worldToCameraMatrix, out var billboardTangent, out var billboardNormal, out var cameraXZAngle);
		data.BillboardNormal = new Vector4(billboardNormal.x, billboardNormal.y, billboardNormal.z, 0f);
		data.BillboardTangent = new Vector4(billboardTangent.x, billboardTangent.y, billboardTangent.z, 0f);
		data.BillboardCameraParams = new Vector4(worldSpaceCameraPos.x, worldSpaceCameraPos.y, worldSpaceCameraPos.z, cameraXZAngle);
	}

	private static void CalculateBillboardProperties(in Matrix4x4 worldToCameraMatrix, out Vector3 billboardTangent, out Vector3 billboardNormal, out float cameraXZAngle)
	{
		Matrix4x4 matrix4x = worldToCameraMatrix;
		matrix4x = matrix4x.transpose;
		Vector3 vector = new Vector3(matrix4x.m00, matrix4x.m10, matrix4x.m20);
		Vector3 vector2 = new Vector3(matrix4x.m01, matrix4x.m11, matrix4x.m21);
		Vector3 lhs = new Vector3(matrix4x.m02, matrix4x.m12, matrix4x.m22);
		Vector3 up = Vector3.up;
		Vector3 vector3 = Vector3.Cross(lhs, up);
		billboardTangent = ((!Mathf.Approximately(vector3.sqrMagnitude, 0f)) ? vector3.normalized : vector);
		billboardNormal = Vector3.Cross(up, billboardTangent);
		billboardNormal = ((!Mathf.Approximately(billboardNormal.sqrMagnitude, 0f)) ? billboardNormal.normalized : vector2);
		Vector3 vector4 = new Vector3(0f, 0f, 1f);
		float y = vector4.x * billboardTangent.z - vector4.z * billboardTangent.x;
		float x = vector4.x * billboardTangent.x + vector4.z * billboardTangent.z;
		cameraXZAngle = Mathf.Atan2(y, x);
		if (cameraXZAngle < 0f)
		{
			cameraXZAngle += MathF.PI * 2f;
		}
	}

	private static void SetupParameters(PassData data)
	{
		WaaaghCameraData cameraData = data.CameraData;
		Camera camera = cameraData.camera;
		float num = cameraData.cameraTargetDescriptor.width;
		float num2 = cameraData.cameraTargetDescriptor.height;
		float num3 = camera.pixelWidth;
		float num4 = camera.pixelHeight;
		if (camera.allowDynamicResolution)
		{
			num *= ScalableBufferManager.widthScaleFactor;
			num2 *= ScalableBufferManager.heightScaleFactor;
		}
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float num5 = (Mathf.Approximately(nearClipPlane, 0f) ? 0f : (1f / nearClipPlane));
		float num6 = (Mathf.Approximately(farClipPlane, 0f) ? 0f : (1f / farClipPlane));
		float w = (camera.orthographic ? 1f : 0f);
		float num7 = 1f - farClipPlane * num5;
		float num8 = farClipPlane * num5;
		Vector4 zBufferParams = new Vector4(num7, num8, num7 * num6, num8 * num6);
		if (SystemInfo.usesReversedZBuffer)
		{
			zBufferParams.y += zBufferParams.x;
			zBufferParams.x = 0f - zBufferParams.x;
			zBufferParams.w += zBufferParams.z;
			zBufferParams.z = 0f - zBufferParams.z;
		}
		if (cameraData.renderType == CameraRenderType.Overlay)
		{
			float x = ((true && SystemInfo.graphicsUVStartsAtTop) ? (-1f) : 1f);
			Vector4 projectionParams = new Vector4(x, nearClipPlane, farClipPlane, 1f * num6);
			data.ProjectionParams = projectionParams;
		}
		data.OrthoParams = new Vector4(camera.orthographicSize * cameraData.aspectRatio, camera.orthographicSize, 0f, w);
		data.WorldSpaceCameraPos = cameraData.worldSpaceCameraPos;
		data.ScreenParams = new Vector4(num3, num4, 1f + 1f / num3, 1f + 1f / num4);
		data.ScaledScreenParams = new Vector4(num, num2, 1f + 1f / num, 1f + 1f / num2);
		data.ZBufferParams = zBufferParams;
		data.ScreenSize = new Vector4(num, num2, 1f / num, 1f / num2);
		data.GlobalMipBias = MipBiasUtils.CalculateGlobalMipBias(cameraData, TemporalAA.GetAutoMipBias(cameraData));
	}

	private static void Execute(PassData data, RasterGraphContext context)
	{
		if (data.CameraData.renderType == CameraRenderType.Base)
		{
			context.cmd.SetupCameraProperties(data.CameraData.camera);
			data.ShaderTimeData.PushGlobal(context.cmd);
			SetCameraMatrices(context.cmd, data, setInverseMatrices: true);
		}
		else
		{
			SetCameraMatrices(context.cmd, data, setInverseMatrices: true);
			SetClippingPlanes(context.cmd, data);
			SetBillboardProperties(context.cmd, data);
		}
		SetParameters(context.cmd, data);
	}

	private static void SetCameraMatrices(RasterCommandBuffer cmd, PassData data, bool setInverseMatrices)
	{
		cmd.SetViewProjectionMatrices(data.ViewMatrix, data.ProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredProjMatrix, data.NonJitteredGpuProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredViewProjMatrix, data.NonJitteredViewProjectionMatrix);
		if (setInverseMatrices)
		{
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_WorldToCamera, data.WorldToCameraMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_CameraToWorld, data.CameraToWorldMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvV, data.InverseViewMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvP, data.InverseProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvVP, data.InverseViewProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId._InvProjMatrix, data.InverseProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyId._InvCameraViewProj, data.InverseViewProjectionMatrix);
			Vector3 vector = data.InverseViewMatrix.GetColumn(1);
			Vector3 vector2 = data.InverseViewMatrix.GetColumn(0);
			Vector3 vector3 = -data.InverseViewMatrix.GetColumn(2);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisUp, vector);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisSide, vector2);
			cmd.SetGlobalVector(ShaderPropertyId._CamBasisFront, vector3);
		}
	}

	private static void SetClippingPlanes(RasterCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalVectorArray(ShaderPropertyId.unity_CameraWorldClipPlanes, data.CameraVectorPlanes);
	}

	private static void SetBillboardProperties(RasterCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardNormal, data.BillboardNormal);
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardTangent, data.BillboardTangent);
		cmd.SetGlobalVector(ShaderPropertyId.unity_BillboardCameraParams, data.BillboardCameraParams);
	}

	private static void SetParameters(RasterCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalVector(ShaderPropertyId._WorldSpaceCameraPos, data.WorldSpaceCameraPos);
		cmd.SetGlobalVector(ShaderPropertyId._ScreenParams, data.ScreenParams);
		cmd.SetGlobalVector(ShaderPropertyId._ScaledScreenParams, data.ScaledScreenParams);
		cmd.SetGlobalVector(ShaderPropertyId._ZBufferParams, data.ZBufferParams);
		cmd.SetGlobalVector(ShaderPropertyId.unity_OrthoParams, data.OrthoParams);
		if (data.CameraData.renderType == CameraRenderType.Overlay)
		{
			cmd.SetGlobalVector(ShaderPropertyId._ProjectionParams, data.ProjectionParams);
		}
		cmd.SetGlobalVector(ShaderPropertyId._ScreenSize, data.ScreenSize);
		cmd.SetGlobalVector(ShaderPropertyId._GlobalMipBias, data.GlobalMipBias);
	}

	public static void ResetJitter(in RecordContext context)
	{
		ResetJitterPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ResetJitterPassData>("Reset Camera Jitter", out passData, WaaaghProfileId.ResetCameraJitter.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\CameraRecorder.cs", 355);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		WaaaghCameraData cameraData = context.CameraData;
		Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
		Matrix4x4 projectionMatrixNoJitter = cameraData.GetProjectionMatrixNoJitter();
		Matrix4x4 gPUProjectionMatrixNoJitter = cameraData.GetGPUProjectionMatrixNoJitter();
		passData.ViewMatrix = viewMatrix;
		passData.ProjectionMatrix = projectionMatrixNoJitter;
		passData.InverseProjectionMatrix = Matrix4x4.Inverse(gPUProjectionMatrixNoJitter);
		passData.InverseViewProjectionMatrix = Matrix4x4.Inverse(viewMatrix) * passData.InverseProjectionMatrix;
		rasterRenderGraphBuilder.SetRenderFunc<ResetJitterPassData>(ExecuteResetJitter);
	}

	private static void ExecuteResetJitter(ResetJitterPassData data, RasterGraphContext context)
	{
		RasterCommandBuffer cmd = context.cmd;
		cmd.SetViewProjectionMatrices(data.ViewMatrix, data.ProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvP, data.InverseProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._InvProjMatrix, data.InverseProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId.unity_MatrixInvVP, data.InverseViewProjectionMatrix);
		cmd.SetGlobalMatrix(ShaderPropertyId._InvCameraViewProj, data.InverseViewProjectionMatrix);
	}

	public static void VFXPrepareCameraPass(in RecordContext context)
	{
		VFXPrepareCameraPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<VFXPrepareCameraPassData>("VFXPrepareCameraPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\CameraRecorder.cs", 390);
		passData.Camera = context.CameraData.camera;
		passData.CullingResults = context.RenderingData.CullResults;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(VFXPrepareCameraPassData data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			VFXManager.ProcessCameraCommand(data.Camera, nativeCommandBuffer, default(VFXCameraXRSettings), data.CullingResults);
		});
	}
}
