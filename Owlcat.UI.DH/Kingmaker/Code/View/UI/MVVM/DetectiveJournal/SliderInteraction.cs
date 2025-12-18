using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class SliderInteraction : Slider
{
	[SerializeField]
	private List<GameObject> m_Highlights;

	private ReactiveProperty<bool> m_IsInteracting;

	private ReactiveProperty<bool> m_HasPointerOverSlider;

	public void Initialize(ReactiveProperty<bool> interactingWithSlider, ReactiveProperty<bool> hasPointerOver)
	{
		m_IsInteracting = interactingWithSlider;
		m_HasPointerOverSlider = hasPointerOver;
		m_IsInteracting.CombineLatest(m_HasPointerOverSlider, (bool interacting, bool hasPointer) => new
		{
			isInteracting = interacting,
			hasPointer = hasPointer
		}).Subscribe(v =>
		{
			UpdateHighlights(v.isInteracting || v.hasPointer);
		}).AddTo(this);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		m_Highlights.ForEach(delegate(GameObject h)
		{
			h.Or(null)?.SetActive(value: true);
		});
		m_HasPointerOverSlider.Value = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		m_HasPointerOverSlider.Value = false;
	}

	public override void OnPointerDown(PointerEventData e)
	{
		base.OnPointerDown(e);
		m_IsInteracting.Value = true;
	}

	public override void OnPointerUp(PointerEventData e)
	{
		m_IsInteracting.Value = false;
	}

	public override void OnDrag(PointerEventData e)
	{
		m_IsInteracting.Value = true;
		base.OnDrag(e);
	}

	private void UpdateHighlights(bool hasHighlight)
	{
		m_Highlights.ForEach(delegate(GameObject h)
		{
			h.Or(null)?.SetActive(hasHighlight);
		});
	}
}
