using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameModes;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;
using UnityEngine;

namespace Kingmaker;

public class NodeLinkVisualizer : MonoBehaviour, IAreaHandler, ISubscriber, IInteractionHighlightUIHandler
{
	private bool m_isInited;

	private WarhammerNodeLink[] m_nodeLinks;

	private List<NodeLinkVisual> m_nodeLinkVisuals = new List<NodeLinkVisual>();

	[SerializeField]
	private NodeLinkVisual NodeLinkVisualPrefab;

	private const int ArcSegments = 12;

	private readonly Vector3[] m_ArcBuffer = new Vector3[13];

	private GameModeType m_GameModeType;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
	}

	private void ToggleAllVisuals(bool state)
	{
		foreach (NodeLinkVisual nodeLinkVisual in m_nodeLinkVisuals)
		{
			if (nodeLinkVisual.NodeLink.IsConnected && !(!nodeLinkVisual.NodeLink.IsActiveAndEnabled && state))
			{
				nodeLinkVisual.gameObject.SetActive(state);
			}
		}
	}

	public void HandleHighlightChange(bool isOn)
	{
		ToggleAllVisuals(isOn);
	}

	public void OnAreaDidLoad()
	{
		Init();
	}

	private void Init()
	{
		m_isInited = false;
		ClearAll();
		if (IsProperGameMode())
		{
			m_nodeLinks = GetNodeLinks();
			m_nodeLinkVisuals = SetupNodeLinks(m_nodeLinks);
			m_isInited = true;
		}
	}

	private void ClearAll()
	{
		foreach (NodeLinkVisual nodeLinkVisual in m_nodeLinkVisuals)
		{
			Object.Destroy(nodeLinkVisual.gameObject);
		}
		m_nodeLinkVisuals.Clear();
		m_nodeLinks = null;
	}

	private List<NodeLinkVisual> SetupNodeLinks(WarhammerNodeLink[] whNodeLinks)
	{
		List<NodeLinkVisual> list = new List<NodeLinkVisual>();
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		foreach (WarhammerNodeLink warhammerNodeLink in whNodeLinks)
		{
			if (warhammerNodeLink.StartNode == null || warhammerNodeLink.EndNode == null)
			{
				PFLog.DesignerDebug.Error((Object)(object)warhammerNodeLink, "Null references in NodeLink traverse. Show this to level designer of this area.");
				continue;
			}
			bool flag = hashSet.Add(warhammerNodeLink.StartNode);
			bool num = hashSet.Add(warhammerNodeLink.EndNode);
			NodeLinkVisual nodeLinkVisual = Object.Instantiate(NodeLinkVisualPrefab, ((Component)(object)warhammerNodeLink).transform.position, Quaternion.identity);
			nodeLinkVisual.Init(warhammerNodeLink);
			nodeLinkVisual.transform.SetParent(base.transform);
			SetupLineRenderers(warhammerNodeLink.StartNode.Vector3Position(), warhammerNodeLink.EndNode.Vector3Position(), nodeLinkVisual);
			SetupNodeDecals(warhammerNodeLink.StartNode.Vector3Position(), warhammerNodeLink.EndNode.Vector3Position(), nodeLinkVisual);
			if (!flag)
			{
				nodeLinkVisual.FirstDecal.SetActive(value: false);
			}
			if (!num)
			{
				nodeLinkVisual.SecondDecal.SetActive(value: false);
			}
			nodeLinkVisual.gameObject.SetActive(value: false);
			list.Add(nodeLinkVisual);
		}
		return list;
	}

	private void SetupNodeDecals(Vector3 a, Vector3 b, NodeLinkVisual nodeLinkVisual)
	{
		nodeLinkVisual.FirstDecal.transform.position = a;
		nodeLinkVisual.SecondDecal.transform.position = b;
		Vector3 vector = b - a;
		vector.y = 0f;
		Vector3 vector2 = a - b;
		vector2.y = 0f;
		if (vector != Vector3.zero)
		{
			nodeLinkVisual.FirstDecal.transform.rotation = SnapRotationTo90(vector);
		}
		if (vector2 != Vector3.zero)
		{
			nodeLinkVisual.SecondDecal.transform.rotation = SnapRotationTo90(vector2);
		}
	}

	private Quaternion SnapRotationTo90(Vector3 flatDirection)
	{
		float y = Mathf.Round(Mathf.Atan2(flatDirection.x, flatDirection.z) * 57.29578f / 90f) * 90f;
		return Quaternion.Euler(0f, y, 0f);
	}

	private void SetupLineRenderers(Vector3 a, Vector3 b, NodeLinkVisual nodeLinkVisual)
	{
		Vector3 rightAngleVerticalPoint = GetRightAngleVerticalPoint(a, b);
		FillQuadraticBezierArc(a, rightAngleVerticalPoint, b);
		nodeLinkVisual.FirstLineRenderer.positionCount = m_ArcBuffer.Length;
		nodeLinkVisual.FirstLineRenderer.SetPositions(m_ArcBuffer);
		nodeLinkVisual.SecondLineRenderer.positionCount = m_ArcBuffer.Length;
		nodeLinkVisual.SecondLineRenderer.SetPositions(m_ArcBuffer);
	}

	private Vector3 GetRightAngleVerticalPoint(Vector3 a, Vector3 b)
	{
		Vector3 vector = ((a.y < b.y) ? a : b);
		float num = Mathf.Abs(b.y - a.y);
		return new Vector3(vector.x, vector.y + num, vector.z);
	}

	private void FillQuadraticBezierArc(Vector3 a, Vector3 controlPoint, Vector3 b)
	{
		for (int i = 0; i <= 12; i++)
		{
			float num = (float)i / 12f;
			float num2 = 1f - num;
			m_ArcBuffer[i] = num2 * num2 * a + 2f * num2 * num * controlPoint + num * num * b;
		}
	}

	private WarhammerNodeLink[] GetNodeLinks()
	{
		return WarhammerNodeLink.All.ToArray();
	}

	private bool IsProperGameMode()
	{
		m_GameModeType = Game.Instance.CurrentModeType;
		if (m_GameModeType != GameModeType.None && m_GameModeType != GameModeType.GlobalMap)
		{
			return m_GameModeType != GameModeType.CutsceneGlobalMap;
		}
		return false;
	}
}
