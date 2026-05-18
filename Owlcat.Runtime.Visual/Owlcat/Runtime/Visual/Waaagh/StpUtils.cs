using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

internal static class StpUtils
{
	internal static TemporalAA.JitterFunc s_JitterFunc = CalculateJitter;

	private static void CalculateJitter(int frameIndex, out Vector2 jitter, out bool allowScaling)
	{
		jitter = -STP.Jit16(frameIndex);
		allowScaling = false;
	}

	public static void PopulateStpConfig(WaaaghCameraData cameraData, in TextureHandle inputColor, in TextureHandle inputDepth, in TextureHandle inputMotion, int debugViewIndex, in TextureHandle debugView, in TextureHandle destination, Texture2D noiseTexture, out STP.Config config)
	{
		cameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component);
		MotionVectorsPersistentData motionVectorsPersistentData = component.MotionVectorsPersistentData;
		config.enableHwDrs = false;
		config.enableTexArray = false;
		config.enableMotionScaling = true;
		int frameCount = Time.frameCount;
		config.noiseTexture = noiseTexture;
		config.inputColor = inputColor;
		config.inputDepth = inputDepth;
		config.inputMotion = inputMotion;
		config.inputStencil = TextureHandle.nullHandle;
		config.stencilMask = 0;
		config.debugView = debugView;
		config.destination = destination;
		StpHistory stpHistory = cameraData.stpHistory;
		int num = 0;
		config.historyContext = stpHistory.GetHistoryContext(num);
		config.nearPlane = cameraData.camera.nearClipPlane;
		config.farPlane = cameraData.camera.farClipPlane;
		config.frameIndex = frameCount;
		config.hasValidHistory = !cameraData.resetHistory;
		config.debugViewIndex = debugViewIndex;
		config.deltaTime = motionVectorsPersistentData.deltaTime;
		config.lastDeltaTime = motionVectorsPersistentData.lastDeltaTime;
		config.currentImageSize = new Vector2Int(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
		config.priorImageSize = config.currentImageSize;
		config.outputImageSize = new Vector2Int(cameraData.pixelWidth, cameraData.pixelHeight);
		int num2 = 1;
		STP.PerViewConfig perViewConfig = default(STP.PerViewConfig);
		for (int i = 0; i < num2; i++)
		{
			int num3 = i + num;
			perViewConfig.currentProj = motionVectorsPersistentData.projectionStereo[num3];
			perViewConfig.lastProj = motionVectorsPersistentData.previousProjectionStereo[num3];
			perViewConfig.lastLastProj = motionVectorsPersistentData.previousPreviousProjectionStereo[num3];
			perViewConfig.currentView = motionVectorsPersistentData.viewStereo[num3];
			perViewConfig.lastView = motionVectorsPersistentData.previousViewStereo[num3];
			perViewConfig.lastLastView = motionVectorsPersistentData.previousPreviousViewStereo[num3];
			Vector3 worldSpaceCameraPos = motionVectorsPersistentData.worldSpaceCameraPos;
			Vector3 previousWorldSpaceCameraPos = motionVectorsPersistentData.previousWorldSpaceCameraPos;
			Vector3 previousPreviousWorldSpaceCameraPos = motionVectorsPersistentData.previousPreviousWorldSpaceCameraPos;
			perViewConfig.currentView.SetColumn(3, new Vector4(0f - worldSpaceCameraPos.x, 0f - worldSpaceCameraPos.y, 0f - worldSpaceCameraPos.z, 1f));
			perViewConfig.lastView.SetColumn(3, new Vector4(0f - previousWorldSpaceCameraPos.x, 0f - previousWorldSpaceCameraPos.y, 0f - previousWorldSpaceCameraPos.z, 1f));
			perViewConfig.lastLastView.SetColumn(3, new Vector4(0f - previousPreviousWorldSpaceCameraPos.x, 0f - previousPreviousWorldSpaceCameraPos.y, 0f - previousPreviousWorldSpaceCameraPos.z, 1f));
			STP.perViewConfigs[i] = perViewConfig;
		}
		config.numActiveViews = num2;
		config.perViewConfigs = STP.perViewConfigs;
	}
}
