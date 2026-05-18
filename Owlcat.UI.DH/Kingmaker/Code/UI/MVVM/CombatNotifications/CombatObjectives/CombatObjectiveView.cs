using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;

public class CombatObjectiveView : View<CombatObjectiveVM>
{
	private const string HighlightedLayerName = "Attention";

	[SerializeField]
	private TMP_Text m_DescriptionText;

	[SerializeField]
	private TMP_Text m_ValueText;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private RectTransform m_HintAnchor;

	[SerializeField]
	private OwlcatMultiSelectable m_HintSource;

	protected override void OnBind()
	{
		m_DescriptionText.text = base.ViewModel.Description.Text;
		base.ViewModel.Value.Subscribe(delegate(string value)
		{
			m_ValueText.text = value;
		}).AddTo(this);
		base.ViewModel.IsActive.Subscribe(delegate(bool active)
		{
			base.gameObject.SetActive(active);
		}).AddTo(this);
		base.ViewModel.State.CombineLatest(base.ViewModel.IsHighlighted, (EncounterObjectiveState state, bool highlighted) => (state: state, highlighted: highlighted)).Subscribe(delegate((EncounterObjectiveState state, bool highlighted) tuple)
		{
			SetActiveLayer(tuple.state, tuple.highlighted);
		}).AddTo(this);
		m_HintSource.OnHoverAsObservable().Subscribe(HandleOnHover).AddTo(this);
	}

	private void HandleOnHover(bool isHover)
	{
		if (isHover)
		{
			base.ViewModel.SetHighlighted(highlighted: false);
		}
		base.ViewModel.ToggleHint(isHover, m_HintAnchor);
	}

	private void SetActiveLayer(EncounterObjectiveState state, bool highlighted)
	{
		m_Selectable.SetActiveLayer(highlighted ? "Attention" : state.ToString());
	}
}
