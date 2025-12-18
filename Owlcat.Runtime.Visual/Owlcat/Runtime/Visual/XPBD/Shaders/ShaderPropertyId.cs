using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Shaders;

public static class ShaderPropertyId
{
	public static readonly int _XpbdBodyIndicesMapOffset;

	public static readonly int _XpbdGravity;

	public static readonly int _XpbdDt;

	public static readonly int _XpbdSubstepDt;

	public static readonly int _XpbdSubsteps;

	public static readonly int _XpbdSubstep;

	public static readonly int _XpbdDeltaTimeRcpSqr;

	public static readonly int _XpbdSleepThreshold;

	public static readonly int _XpbdColliderCollisionsEnabled;

	public static readonly int _XpbdParticleCollisionsEnabled;

	public static readonly int _XpbdCollisionMaxDepenetration;

	public static readonly int _XpbdBodyGroupsMapOffset;

	public static readonly int _XpbdParticlesBasePositionBuffer;

	public static readonly int _XpbdParticlesPositionBuffer;

	public static readonly int _XpbdParticlesOrientationBuffer;

	public static readonly int _XpbdParticlesVelocityBuffer;

	public static readonly int _XpbdParticlesRadiusBuffer;

	public static readonly int _XpbdParticlesMap;

	public static readonly int _XpbdDeactivatedParticlesCount;

	public static readonly int _XpbdConstraintsIndices;

	public static readonly int _XpbdConstraintsParameters0;

	public static readonly int _XpbdConstraintsParameters1;

	public static readonly int _XpbdConstraintsMap;

	public static readonly int _XpbdBodyVertexPositionBuffer;

	public static readonly int _XpbdBodyVertexNormalBuffer;

	public static readonly int SkinnedMeshConstantBuffer;

	public static readonly int _XpbdSkinnedBodyIndices;

	public static readonly int _XpbdSkinnedBodyLocalToWorld;

	public static readonly int[] _XpbdSkinnedVertexBuffers;

	public static readonly int _XpbdBodyBoneSimulatedBindposeBuffer;

	public static readonly int _XpbdBoneIndicesMapBuffer;

	public static readonly int _XpbdContactsCountTotal;

	public static readonly int _XpbdHashmapSize;

	public static readonly int _XpbdSpatialHashmapMetricsObjectSizeSum;

	public static readonly int _XpbdSpatialHashmapMetricsOccupiedCellsCount;

	public static readonly int _XpbdSpatialHashmapMetricsActiveContactsCount;

	public static readonly int _XpbdSpatialHashmapObjectsCount;

	public static readonly int _XpbdSpatialHashmapSizeDescretizer;

	public static readonly int _XpbdSpatialHashmapSpacingScale;

	public static readonly int _XpbdSpatialHashmapMaxHashtableLookupIterations;

	public static readonly int _XpbdContinousCollisionDetection;

	public static readonly int _XpbdCollisionMargin;

	public static readonly int _XpbdGenerateContactsIterations;

	public static readonly int _XpbdGenerateContactsTolerance;

	public static readonly int _XpbdMaxContactsCount;

	public static readonly int _XpbdEnabledGlobal;

	public static readonly int _XpbdEnabledLocal;

	public static readonly int _XpbdVertexToParticleMapBuffer;

	public static readonly int _XpbdVertexToParticleMapOffset;

	public static readonly int _XpbdBodyWorldToLocal;

	public static readonly int _XpbdVertexOffset;

	public static readonly int _XpbdBoneIndicesOffset;

	public static readonly int _XbdBonesOffset;

	public static readonly int _XpbdDeformableVerticesOffset;

	public static readonly int _XpbdDeformableVerticesBuffer;

	public static readonly int _XpbdDeformableSkinnedVerticesOffset;

	public static readonly int _XpbdDeformableSkinnedVerticesBuffer;

	public static readonly int _XpbdVertexToDeformableVertexMapBuffer;

	public static readonly int _XpbdVertexToDeformableVertexMapOffset;

	public static readonly int _Color;

	public static readonly int _ColorSecondary;

	public static readonly int _Tint;

	public static readonly int _UseTint;

	public static readonly int _XpbdUseParticleMap;

	public static readonly int _ParticleHandleSize;

	public static readonly int _XpbdDebugDrawRestNormals;

	public static readonly int _XpbdConstraintLinesCount;

	public static readonly int _XpbdConstraintParticlesCount;

	public static readonly int _XpbdDebugAabbBuffer;

	public static readonly int _XpbdDebugAabbMap;

	public static readonly int _XpbdConstraintOffset;

	public static readonly int _XpbdDebugConstructAabbFromConstraintParameters;

	public static readonly int _XpbdParticlesOffset;

	public static readonly int _XpbdBodyLocalToWorld;

	public static readonly int _XpbdInertialLinearAccel;

	public static readonly int _XpbdInertialAngularVel;

	public static readonly int _XpbdInertialAngularAccel;

	public static readonly int _XpbdParticlesCount;

	public static readonly int _XpbdDebugUseVisibilityBuffer;

	public static readonly int _XpbdDebugAabbVisibilityBuffer;

	static ShaderPropertyId()
	{
		_XpbdBodyIndicesMapOffset = Shader.PropertyToID("_XpbdBodyIndicesMapOffset");
		_XpbdGravity = Shader.PropertyToID("_XpbdGravity");
		_XpbdDt = Shader.PropertyToID("_XpbdDt");
		_XpbdSubstepDt = Shader.PropertyToID("_XpbdSubstepDt");
		_XpbdSubsteps = Shader.PropertyToID("_XpbdSubsteps");
		_XpbdSubstep = Shader.PropertyToID("_XpbdSubstep");
		_XpbdDeltaTimeRcpSqr = Shader.PropertyToID("_XpbdDeltaTimeRcpSqr");
		_XpbdSleepThreshold = Shader.PropertyToID("_XpbdSleepThreshold");
		_XpbdColliderCollisionsEnabled = Shader.PropertyToID("_XpbdColliderCollisionsEnabled");
		_XpbdParticleCollisionsEnabled = Shader.PropertyToID("_XpbdParticleCollisionsEnabled");
		_XpbdCollisionMaxDepenetration = Shader.PropertyToID("_XpbdCollisionMaxDepenetration");
		_XpbdBodyGroupsMapOffset = Shader.PropertyToID("_XpbdBodyGroupsMapOffset");
		_XpbdParticlesBasePositionBuffer = Shader.PropertyToID("_XpbdParticlesBasePositionBuffer");
		_XpbdParticlesPositionBuffer = Shader.PropertyToID("_XpbdParticlesPositionBuffer");
		_XpbdParticlesOrientationBuffer = Shader.PropertyToID("_XpbdParticlesOrientationBuffer");
		_XpbdParticlesVelocityBuffer = Shader.PropertyToID("_XpbdParticlesVelocityBuffer");
		_XpbdParticlesRadiusBuffer = Shader.PropertyToID("_XpbdParticlesRadiusBuffer");
		_XpbdParticlesMap = Shader.PropertyToID("_XpbdParticlesMap");
		_XpbdDeactivatedParticlesCount = Shader.PropertyToID("_XpbdDeactivatedParticlesCount");
		_XpbdConstraintsIndices = Shader.PropertyToID("_XpbdConstraintsIndices");
		_XpbdConstraintsParameters0 = Shader.PropertyToID("_XpbdConstraintsParameters0");
		_XpbdConstraintsParameters1 = Shader.PropertyToID("_XpbdConstraintsParameters1");
		_XpbdConstraintsMap = Shader.PropertyToID("_XpbdConstraintsMap");
		_XpbdBodyVertexPositionBuffer = Shader.PropertyToID("_XpbdBodyVertexPositionBuffer");
		_XpbdBodyVertexNormalBuffer = Shader.PropertyToID("_XpbdBodyVertexNormalBuffer");
		SkinnedMeshConstantBuffer = Shader.PropertyToID("SkinnedMeshConstantBuffer");
		_XpbdSkinnedBodyIndices = Shader.PropertyToID("_XpbdSkinnedBodyIndices");
		_XpbdSkinnedBodyLocalToWorld = Shader.PropertyToID("_XpbdSkinnedBodyLocalToWorld");
		_XpbdBodyBoneSimulatedBindposeBuffer = Shader.PropertyToID("_XpbdBodyBoneSimulatedBindposeBuffer");
		_XpbdBoneIndicesMapBuffer = Shader.PropertyToID("_XpbdBoneIndicesMapBuffer");
		_XpbdContactsCountTotal = Shader.PropertyToID("_XpbdContactsCountTotal");
		_XpbdHashmapSize = Shader.PropertyToID("_XpbdHashmapSize");
		_XpbdSpatialHashmapMetricsObjectSizeSum = Shader.PropertyToID("_XpbdSpatialHashmapMetricsObjectSizeSum");
		_XpbdSpatialHashmapMetricsOccupiedCellsCount = Shader.PropertyToID("_XpbdSpatialHashmapMetricsOccupiedCellsCount");
		_XpbdSpatialHashmapMetricsActiveContactsCount = Shader.PropertyToID("_XpbdSpatialHashmapMetricsActiveContactsCount");
		_XpbdSpatialHashmapObjectsCount = Shader.PropertyToID("_XpbdSpatialHashmapObjectsCount");
		_XpbdSpatialHashmapSizeDescretizer = Shader.PropertyToID("_XpbdSpatialHashmapSizeDescretizer");
		_XpbdSpatialHashmapSpacingScale = Shader.PropertyToID("_XpbdSpatialHashmapSpacingScale");
		_XpbdSpatialHashmapMaxHashtableLookupIterations = Shader.PropertyToID("_XpbdSpatialHashmapMaxHashtableLookupIterations");
		_XpbdContinousCollisionDetection = Shader.PropertyToID("_XpbdContinousCollisionDetection");
		_XpbdCollisionMargin = Shader.PropertyToID("_XpbdCollisionMargin");
		_XpbdGenerateContactsIterations = Shader.PropertyToID("_XpbdGenerateContactsIterations");
		_XpbdGenerateContactsTolerance = Shader.PropertyToID("_XpbdGenerateContactsTolerance");
		_XpbdMaxContactsCount = Shader.PropertyToID("_XpbdMaxContactsCount");
		_XpbdEnabledGlobal = Shader.PropertyToID("_XpbdEnabledGlobal");
		_XpbdEnabledLocal = Shader.PropertyToID("_XpbdEnabledLocal");
		_XpbdVertexToParticleMapBuffer = Shader.PropertyToID("_XpbdVertexToParticleMapBuffer");
		_XpbdVertexToParticleMapOffset = Shader.PropertyToID("_XpbdVertexToParticleMapOffset");
		_XpbdBodyWorldToLocal = Shader.PropertyToID("_XpbdBodyWorldToLocal");
		_XpbdVertexOffset = Shader.PropertyToID("_XpbdVertexOffset");
		_XpbdBoneIndicesOffset = Shader.PropertyToID("_XpbdBoneIndicesOffset");
		_XbdBonesOffset = Shader.PropertyToID("_XbdBonesOffset");
		_XpbdDeformableVerticesOffset = Shader.PropertyToID("_XpbdDeformableVerticesOffset");
		_XpbdDeformableVerticesBuffer = Shader.PropertyToID("_XpbdDeformableVerticesBuffer");
		_XpbdDeformableSkinnedVerticesOffset = Shader.PropertyToID("_XpbdDeformableSkinnedVerticesOffset");
		_XpbdDeformableSkinnedVerticesBuffer = Shader.PropertyToID("_XpbdDeformableSkinnedVerticesBuffer");
		_XpbdVertexToDeformableVertexMapBuffer = Shader.PropertyToID("_XpbdVertexToDeformableVertexMapBuffer");
		_XpbdVertexToDeformableVertexMapOffset = Shader.PropertyToID("_XpbdVertexToDeformableVertexMapOffset");
		_Color = Shader.PropertyToID("_Color");
		_ColorSecondary = Shader.PropertyToID("_ColorSecondary");
		_Tint = Shader.PropertyToID("_Tint");
		_UseTint = Shader.PropertyToID("_UseTint");
		_XpbdUseParticleMap = Shader.PropertyToID("_XpbdUseParticleMap");
		_ParticleHandleSize = Shader.PropertyToID("_ParticleHandleSize");
		_XpbdDebugDrawRestNormals = Shader.PropertyToID("_XpbdDebugDrawRestNormals");
		_XpbdConstraintLinesCount = Shader.PropertyToID("_XpbdConstraintLinesCount");
		_XpbdConstraintParticlesCount = Shader.PropertyToID("_XpbdConstraintParticlesCount");
		_XpbdDebugAabbBuffer = Shader.PropertyToID("_XpbdDebugAabbBuffer");
		_XpbdDebugAabbMap = Shader.PropertyToID("_XpbdDebugAabbMap");
		_XpbdConstraintOffset = Shader.PropertyToID("_XpbdConstraintOffset");
		_XpbdDebugConstructAabbFromConstraintParameters = Shader.PropertyToID("_XpbdDebugConstructAabbFromConstraintParameters");
		_XpbdParticlesOffset = Shader.PropertyToID("_XpbdParticlesOffset");
		_XpbdBodyLocalToWorld = Shader.PropertyToID("_XpbdBodyLocalToWorld");
		_XpbdInertialLinearAccel = Shader.PropertyToID("_XpbdInertialLinearAccel");
		_XpbdInertialAngularVel = Shader.PropertyToID("_XpbdInertialAngularVel");
		_XpbdInertialAngularAccel = Shader.PropertyToID("_XpbdInertialAngularAccel");
		_XpbdParticlesCount = Shader.PropertyToID("_XpbdParticlesCount");
		_XpbdDebugUseVisibilityBuffer = Shader.PropertyToID("_XpbdDebugUseVisibilityBuffer");
		_XpbdDebugAabbVisibilityBuffer = Shader.PropertyToID("_XpbdDebugAabbVisibilityBuffer");
		_XpbdSkinnedVertexBuffers = new int[26];
		for (int i = 0; i < 26; i++)
		{
			_XpbdSkinnedVertexBuffers[i] = Shader.PropertyToID($"_XpbdSkinnedVertexBuffer{i}");
		}
	}
}
