using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public sealed class CombatHudPathRenderer : MonoBehaviour, IAreaHandler, ISubscriber
{
	[Serializable]
	public struct Config
	{
		public Material normalMaterial;

		public Material unableMaterial;

		public Material threateningMaterial;

		public PathLineSettings lineSettings;

		public LineMaterialSettingsPerCreatureSize[] lineMaterialSettingsPerCreatureSize;

		public bool IsValid()
		{
			if (normalMaterial != null && unableMaterial != null)
			{
				return threateningMaterial != null;
			}
			return false;
		}
	}

	[Serializable]
	public struct LineMaterialSettingsPerCreatureSize
	{
		public Size creatureSize;

		public float fadeStartOffset;

		public float fadeEndOffset;

		public float fadeSharpness;
	}

	[Header("Components")]
	[SerializeField]
	private MeshFilter m_PathMeshFilter;

	[SerializeField]
	private MeshRenderer m_PathMeshRenderer;

	[Header("Settings")]
	[SerializeField]
	private Config m_SurfaceCombatConfig;

	[SerializeField]
	private Config m_SpaceCombatConfig;

	[SerializeField]
	private Vector3 m_MeshOffset;

	private PathService m_Service;

	private readonly CombatHubCollectionAreaSource m_Source = new CombatHubCollectionAreaSource();

	private readonly PathServiceRequestPool m_RequestPool = new PathServiceRequestPool();

	private bool m_UseSpaceCombatConfig;

	public bool PathShown { get; private set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		if (m_Service != null)
		{
			m_Service.Dispose();
			m_Service = null;
		}
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		m_Service?.Update();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		m_UseSpaceCombatConfig = false;
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	public void Show(List<GraphNode> nodes, Transform progressTargetTransform, bool pathUnableStatus, bool threateningStatus, Vector3 meshOffset, Size creatureSize = Size.Medium)
	{
		GridGraph graph = CombatHudGraphDataSource.FindGraph();
		if (EnsurePrerequisites(graph))
		{
			m_Source.Clear();
			if (nodes != null)
			{
				m_Source.AddRange(nodes);
			}
			m_Service.DiscardPendingRequest();
			PathServiceRequest pathServiceRequest = m_RequestPool.Get();
			pathServiceRequest.GridSettings = new GridSettings(graph);
			pathServiceRequest.Graph = graph;
			pathServiceRequest.PathLineSettings = GetConfig().lineSettings;
			pathServiceRequest.source = m_Source;
			pathServiceRequest.material = GetMaterial(pathUnableStatus, threateningStatus);
			pathServiceRequest.positionOffset = m_MeshOffset + meshOffset;
			pathServiceRequest.progressTrackingTransform = progressTargetTransform;
			pathServiceRequest.FadeSettings = FindFadeSettings(creatureSize);
			m_Service.SetPendingRequest(pathServiceRequest);
			PathShown = !nodes.Empty();
		}
	}

	private LineMaterialSettingsPerCreatureSize? FindFadeSettings(Size creatureSize)
	{
		LineMaterialSettingsPerCreatureSize[] lineMaterialSettingsPerCreatureSize = GetConfig().lineMaterialSettingsPerCreatureSize;
		if (lineMaterialSettingsPerCreatureSize == null)
		{
			return null;
		}
		for (int i = 0; i < lineMaterialSettingsPerCreatureSize.Length; i++)
		{
			if (lineMaterialSettingsPerCreatureSize[i].creatureSize == creatureSize)
			{
				return lineMaterialSettingsPerCreatureSize[i];
			}
		}
		return null;
	}

	private Material GetMaterial(bool pathUnableStatus, bool threateningStatus)
	{
		Config config = GetConfig();
		if (pathUnableStatus)
		{
			return config.unableMaterial;
		}
		if (threateningStatus)
		{
			return config.threateningMaterial;
		}
		return config.normalMaterial;
	}

	private bool EnsurePrerequisites(GridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (m_PathMeshFilter == null)
		{
			return false;
		}
		if (m_PathMeshRenderer == null)
		{
			return false;
		}
		if (!GetConfig().IsValid())
		{
			return false;
		}
		if (m_Service == null)
		{
			m_Service = new PathService(m_PathMeshFilter, m_PathMeshRenderer, m_RequestPool);
		}
		return true;
	}

	private ref Config GetConfig()
	{
		if (m_UseSpaceCombatConfig)
		{
			return ref m_SpaceCombatConfig;
		}
		return ref m_SurfaceCombatConfig;
	}
}
