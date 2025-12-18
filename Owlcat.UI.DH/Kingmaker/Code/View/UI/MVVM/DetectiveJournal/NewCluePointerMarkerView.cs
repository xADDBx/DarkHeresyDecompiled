using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewCluePointerMarkerView : View<INewIconTarget>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private RectTransform m_RotationTransform;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[Header("Values")]
	[SerializeField]
	private float m_MinScale = 0.5f;

	[SerializeField]
	private float m_MaxScale = 1f;

	[SerializeField]
	private float m_WorldLength = 1f;

	public RectTransform NewClueMarker { get; private set; }

	protected override void OnBind()
	{
		base.ViewModel.NewIcon.Subscribe(delegate(RectTransform value)
		{
			NewClueMarker = value;
			SetActiveState(value != null);
		}).AddTo(this);
		m_Button.OnLeftClickAsObservable().Subscribe(MoveToClueView).AddTo(this);
	}

	private void SetActiveState(bool value)
	{
		base.gameObject.SetActive(value);
	}

	private void SetVisibleState(bool state)
	{
		m_CanvasGroup.alpha = (state ? 1 : 0);
	}

	public void SetTransformValues(Vector3? position, float currentScale)
	{
		SetVisibleState(position.HasValue);
		if (position.HasValue)
		{
			float a = Mathf.Max(m_MinScale, currentScale * m_MaxScale);
			base.transform.position = position.Value;
			float z = Vector3.SignedAngle(Vector3.down, NewClueMarker.position - position.Value, Vector3.forward);
			m_RotationTransform.localEulerAngles = new Vector3(0f, 0f, z);
			float num = Mathf.Lerp(t: (position.Value - NewClueMarker.position).magnitude / m_WorldLength, a: a, b: m_MinScale);
			base.transform.localScale = Vector3.one * num;
		}
	}

	private void MoveToClueView()
	{
		EventBus.RaiseEvent(delegate(IMoveToCaseItemHandler h)
		{
			h.HandleMoveToItemPosition(base.ViewModel.Parent.position);
		});
	}
}
