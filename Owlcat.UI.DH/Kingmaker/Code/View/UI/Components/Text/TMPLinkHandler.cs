using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Text;

public class TMPLinkHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Serializable]
	public class LinkEventData : UnityEvent<PointerEventData, TMP_LinkInfo>
	{
	}

	protected enum TransitionState
	{
		Normal,
		Highlighted,
		Pressed
	}

	[Serializable]
	public class LinkHandlerSettings
	{
		[field: Header("Text Color")]
		[field: SerializeField]
		public bool ColorTransition { get; private set; } = true;


		[field: SerializeField]
		[field: ShowIf("ColorTransition")]
		public bool OverlayTextNormalColor { get; private set; }

		[field: SerializeField]
		[field: ShowIf("ColorTransition")]
		public Color NormalTextColor { get; private set; } = Color.white;


		[field: SerializeField]
		[field: ShowIf("ColorTransition")]
		public Color HighlightedTextColor { get; private set; } = Color.blue;


		[field: Header("Mark Color")]
		[field: SerializeField]
		public bool MarkTransition { get; private set; } = true;


		[field: SerializeField]
		[field: ShowIf("MarkTransition")]
		public RectTransform MarkPrefab { get; private set; }

		[field: SerializeField]
		[field: ShowIf("MarkTransition")]
		public Vector2 MarkPadding { get; private set; }

		[field: SerializeField]
		[field: ShowIf("MarkTransition")]
		public bool DisplayMarkBehind { get; private set; }

		[field: Header("Underline")]
		[field: SerializeField]
		public bool HighlightedUnderline { get; private set; } = true;

	}

	[SerializeField]
	protected bool m_Interactable = true;

	[SerializeField]
	protected LinkHandlerSettings m_Settings = new LinkHandlerSettings();

	private TextMeshProUGUI m_TextComponent;

	private RectTransform m_RectTransform;

	private readonly List<RectTransform> m_MarkObjectsPool = new List<RectTransform>();

	private PointerEventData m_CurrentEventData;

	private int m_HoveredLink = -1;

	private int m_DownIndex = -1;

	[field: Space(10f)]
	[field: SerializeField]
	public LinkEventData OnClick { get; private set; } = new LinkEventData();


	[field: SerializeField]
	public LinkEventData OnEnter { get; private set; } = new LinkEventData();


	[field: SerializeField]
	public LinkEventData OnHover { get; private set; } = new LinkEventData();


	[field: SerializeField]
	public LinkEventData OnExit { get; private set; } = new LinkEventData();


	public bool IsHover => m_CurrentEventData != null;

	protected void OnEnable()
	{
		m_TextComponent = GetComponent<TextMeshProUGUI>();
		m_RectTransform = GetComponent<RectTransform>();
		DelayedInvoker.InvokeInFrames(ResetTransition, 2).AddTo(this);
	}

	protected void OnDisable()
	{
		if (m_HoveredLink >= 0)
		{
			CallEvent(OnExit, m_CurrentEventData, m_TextComponent.textInfo.linkInfo[m_HoveredLink]);
			m_HoveredLink = -1;
		}
		m_DownIndex = -1;
		m_MarkObjectsPool.ForEach(delegate(RectTransform o)
		{
			UnityEngine.Object.Destroy(o.gameObject);
		});
		m_MarkObjectsPool.Clear();
	}

	private void Reset()
	{
		if ((object)m_TextComponent == null)
		{
			m_TextComponent = GetComponent<TextMeshProUGUI>();
		}
		TMP_CharacterInfo[] characterInfo = m_TextComponent.textInfo.characterInfo;
		for (int i = 0; i < characterInfo.Length; i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = characterInfo[i];
			for (int j = 0; j < 4; j++)
			{
				m_TextComponent.textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex].colors32[tMP_CharacterInfo.vertexIndex + j] = Color.Lerp(tMP_CharacterInfo.color * Color.white, Color.white, 0f);
			}
		}
		m_TextComponent.UpdateVertexData((TMP_VertexDataUpdateFlags)17);
		m_MarkObjectsPool.ForEach(delegate(RectTransform o)
		{
			UnityEngine.Object.Destroy(o.gameObject);
		});
		m_MarkObjectsPool.Clear();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_Interactable)
		{
			m_DownIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, eventData.position, eventData.enterEventCamera);
			if (m_DownIndex >= 0)
			{
				TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_DownIndex];
				DoTransition(info, TransitionState.Pressed);
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_Interactable && m_DownIndex >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_DownIndex];
			CallEvent(OnClick, eventData, info);
			m_DownIndex = -1;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_Interactable)
		{
			m_CurrentEventData = eventData;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_CurrentEventData = null;
		if (m_HoveredLink >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_HoveredLink];
			CallEvent(OnExit, eventData, info);
			DoTransition(info, TransitionState.Normal);
			m_HoveredLink = -1;
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.TmpDisabled && !focus && m_HoveredLink >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_HoveredLink];
			CallEvent(OnExit, m_CurrentEventData, info);
			DoTransition(info, TransitionState.Normal);
			m_HoveredLink = -1;
		}
	}

	public void Update()
	{
		if (m_TextComponent == null)
		{
			return;
		}
		if (!m_Interactable)
		{
			m_CurrentEventData = null;
			return;
		}
		TMP_LinkInfo[] linkInfo = m_TextComponent.textInfo.linkInfo;
		if (!IsHover)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, m_CurrentEventData.position, m_CurrentEventData.enterEventCamera);
		if (num != m_HoveredLink)
		{
			if (m_HoveredLink >= 0)
			{
				TMP_LinkInfo info = linkInfo[m_HoveredLink];
				CallEvent(OnExit, m_CurrentEventData, info);
				DoTransition(info, TransitionState.Normal);
			}
			m_HoveredLink = num;
			if (m_HoveredLink >= 0)
			{
				TMP_LinkInfo info2 = linkInfo[m_HoveredLink];
				CallEvent(OnEnter, m_CurrentEventData, info2);
				DoTransition(info2, TransitionState.Highlighted);
			}
		}
		if (m_HoveredLink >= 0)
		{
			TMP_LinkInfo info3 = linkInfo[m_HoveredLink];
			CallEvent(OnHover, m_CurrentEventData, info3);
		}
	}

	protected void DoTransition(TMP_LinkInfo info, TransitionState state)
	{
		TransitionWord(info.linkTextfirstCharacterIndex, info.linkTextLength, state);
	}

	public void ResetTransition()
	{
		TMP_LinkInfo[] linkInfo = m_TextComponent.textInfo.linkInfo;
		if (linkInfo != null && linkInfo.Length != 0 && m_TextComponent.textInfo.linkCount > 0)
		{
			TMP_LinkInfo[] array = linkInfo;
			for (int i = 0; i < array.Length; i++)
			{
				TMP_LinkInfo tMP_LinkInfo = array[i];
				TransitionWord(tMP_LinkInfo.linkTextfirstCharacterIndex, tMP_LinkInfo.linkTextLength, TransitionState.Normal);
			}
		}
	}

	private void TransitionWord(int index, int lenght, TransitionState state)
	{
		Color color = state switch
		{
			TransitionState.Normal => m_Settings.NormalTextColor, 
			TransitionState.Highlighted => m_Settings.HighlightedTextColor, 
			TransitionState.Pressed => m_Settings.NormalTextColor, 
			_ => m_Settings.NormalTextColor, 
		};
		int num = state switch
		{
			TransitionState.Normal => m_Settings.OverlayTextNormalColor ? 1 : 0, 
			TransitionState.Highlighted => m_Settings.OverlayTextNormalColor ? 1 : 0, 
			TransitionState.Pressed => m_Settings.OverlayTextNormalColor ? 1 : 0, 
			_ => 0, 
		};
		TMP_CharacterInfo[] characterInfo = m_TextComponent.textInfo.characterInfo;
		if (characterInfo == null)
		{
			return;
		}
		for (int i = index; i < Mathf.Min(index + lenght, characterInfo.Length); i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = m_TextComponent.textInfo.characterInfo[i];
			if (!tMP_CharacterInfo.isVisible)
			{
				continue;
			}
			if (m_Settings.HighlightedUnderline)
			{
				tMP_CharacterInfo.style |= FontStyles.Underline;
			}
			else
			{
				tMP_CharacterInfo.style &= ~FontStyles.Underline;
			}
			int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
			int vertexIndex = tMP_CharacterInfo.vertexIndex;
			if (m_Settings.ColorTransition)
			{
				for (int j = 0; j < 4; j++)
				{
					m_TextComponent.textInfo.meshInfo[materialReferenceIndex].colors32[vertexIndex + j] = Color.Lerp(tMP_CharacterInfo.color * color, color, (float)num);
				}
			}
		}
		UpdateMark(index, lenght, state);
		m_TextComponent.UpdateVertexData((TMP_VertexDataUpdateFlags)17);
	}

	private void UpdateMark(int index, int lenght, TransitionState state)
	{
		if (!m_Settings.MarkTransition)
		{
			return;
		}
		TMP_TextInfo textInfo = m_TextComponent.textInfo;
		if (state == TransitionState.Normal)
		{
			m_MarkObjectsPool.ForEach(delegate(RectTransform o)
			{
				o.gameObject.SetActive(value: false);
			});
			return;
		}
		TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[index];
		int num = index + lenght - 1;
		TMP_CharacterInfo tMP_CharacterInfo2 = textInfo.characterInfo[num];
		int lineNumber = tMP_CharacterInfo.lineNumber;
		int num2 = tMP_CharacterInfo2.lineNumber - tMP_CharacterInfo.lineNumber + 1;
		for (int i = m_MarkObjectsPool.Count; i < num2; i++)
		{
			Transform parent = (m_Settings.DisplayMarkBehind ? base.transform.parent : base.transform);
			int siblingIndex = (m_Settings.DisplayMarkBehind ? base.transform.GetSiblingIndex() : 0);
			RectTransform rectTransform = UnityEngine.Object.Instantiate(m_Settings.MarkPrefab, parent, worldPositionStays: false);
			rectTransform.SetSiblingIndex(siblingIndex);
			if (m_Settings.DisplayMarkBehind)
			{
				LayoutElement component = rectTransform.gameObject.GetComponent<LayoutElement>();
				component = ((component == null) ? rectTransform.gameObject.AddComponent<LayoutElement>() : component);
				component.ignoreLayout = true;
			}
			m_MarkObjectsPool.Add(rectTransform);
		}
		for (int j = 0; j < num2; j++)
		{
			int firstLetterId = ((j == tMP_CharacterInfo.lineNumber - lineNumber) ? index : (GetLastLetterIdInLine(index, lineNumber + j) + 1));
			int lastLetterId = ((j == tMP_CharacterInfo2.lineNumber - lineNumber) ? num : GetLastLetterIdInLine(index, lineNumber + j + 1));
			RectTransform rectTransform2 = m_MarkObjectsPool.ElementAt(j);
			CalculateGlossaryPointCoordinates(firstLetterId, lastLetterId, out var height, out var width, out var middlePoint);
			rectTransform2.position = m_RectTransform.TransformPoint(middlePoint);
			rectTransform2.sizeDelta = new Vector2(width, height) + m_Settings.MarkPadding;
			rectTransform2.gameObject.SetActive(value: true);
		}
	}

	private int GetLastLetterIdInLine(int startId, int lineNumber)
	{
		for (int i = startId; i < m_TextComponent.textInfo.characterCount; i++)
		{
			if (m_TextComponent.textInfo.characterInfo[i].lineNumber >= lineNumber)
			{
				return i - 1;
			}
		}
		return -1;
	}

	private void CalculateGlossaryPointCoordinates(int firstLetterId, int lastLetterId, out float height, out float width, out Vector2 middlePoint)
	{
		TMP_TextInfo textInfo = m_TextComponent.textInfo;
		TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[firstLetterId];
		TMP_CharacterInfo tMP_CharacterInfo2 = textInfo.characterInfo[lastLetterId];
		float y = tMP_CharacterInfo.bottomRight.y;
		float y2 = tMP_CharacterInfo.topRight.y;
		for (int i = firstLetterId + 1; i <= lastLetterId; i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo3 = textInfo.characterInfo[i];
			if (y > tMP_CharacterInfo3.bottomRight.y)
			{
				y = tMP_CharacterInfo3.bottomRight.y;
			}
			if (y2 < tMP_CharacterInfo3.topRight.y)
			{
				y2 = tMP_CharacterInfo3.topRight.y;
			}
		}
		height = Mathf.Abs(y2 - y);
		width = tMP_CharacterInfo2.topRight.x - tMP_CharacterInfo.topLeft.x;
		middlePoint = new Vector2(tMP_CharacterInfo.topLeft.x + width / 2f, y2 - height / 2f);
	}

	public void CallEvent(LinkEventData linkEvent, PointerEventData eventData, TMP_LinkInfo info)
	{
		linkEvent?.Invoke(eventData, info);
	}
}
