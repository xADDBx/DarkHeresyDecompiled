using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseScreenBaseView : View<OpenedCaseScreenVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_ToggleAnnotationsButton;

	[SerializeField]
	private List<CanvasGroup> m_CanvasGroups;

	[Header("Views")]
	[SerializeField]
	private OpenedCaseAnnotationsView m_AnnotationsView;

	[SerializeField]
	private OpenedCaseTransformControlsBaseView TransformControlsBaseView;

	protected override void OnBind()
	{
		m_ToggleAnnotationsButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.ToggleAnnotations).AddTo(this);
		base.ViewModel.AnnotationsVM.Subscribe(delegate(OpenedCaseAnnotationsVM value)
		{
			m_ToggleAnnotationsButton.SetActiveLayer((value == null) ? "ButtonOff" : "ButtonOn");
			m_AnnotationsView.Bind(value);
		}).AddTo(this);
		base.ViewModel.OpenedCaseTransformControlsVM.Subscribe(delegate(OpenedCaseTransformControlsVM value)
		{
			TransformControlsBaseView.Bind(value);
		}).AddTo(this);
	}

	public void SetVisibleState(bool state)
	{
		m_CanvasGroups.ForEach(delegate(CanvasGroup cg)
		{
			cg.alpha = (state ? 1 : 0);
			cg.interactable = state;
			cg.blocksRaycasts = state;
		});
	}

	public void HandleMoveToPosition(Vector3 worldPos)
	{
		TransformControlsBaseView.HandleMoveToPosition(worldPos);
	}
}
