using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConstructConclusionView : View<ConstructConclusionVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_NewConclusionsSelectable;

	[SerializeField]
	private TMP_Text m_NewConclusionsCount;

	[field: Header("Elements")]
	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	public bool HasNewConclusion => base.ViewModel.HasNewConclusion.CurrentValue;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		m_Button.OnPointerEnterAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnHover();
		}).AddTo(this);
		m_Button.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnHoverEnd();
		}).AddTo(this);
		base.ViewModel.HasNewConclusion.Subscribe(delegate(bool value)
		{
			string activeLayer = "New";
			if (!value)
			{
				activeLayer = (base.ViewModel.IsRefuted ? "AllRefuted" : "Default");
			}
			m_StateSelectable.SetActiveLayer(activeLayer);
		}).AddTo(this);
		base.ViewModel.NewConclusionsCount.Subscribe(delegate(int value)
		{
			m_NewConclusionsSelectable.SetActiveLayer((value > 0) ? 1 : 0);
			m_NewConclusionsCount.text = $"+{value}";
		}).AddTo(this);
	}
}
