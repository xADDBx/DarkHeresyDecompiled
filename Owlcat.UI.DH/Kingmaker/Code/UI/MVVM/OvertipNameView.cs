using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipNameView : View<LightweightOvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	private CanvasRenderer m_CharacterNameCanvasRenderer;

	private CanvasRenderer CharacterNameCanvasRenderer => m_CharacterNameCanvasRenderer ?? (m_CharacterNameCanvasRenderer = m_CharacterName.GetComponent<CanvasRenderer>());

	protected override void OnBind()
	{
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_CharacterName.text = value;
		}).AddTo(this);
		if (m_MultiSelectable != null)
		{
			base.ViewModel.MechanicEntityUIState.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.MechanicEntityUIState.IsPlayer.CurrentValue ? "Party" : "Ally"));
			}).AddTo(this);
		}
		m_CharacterName.SetHint(base.ViewModel.MechanicEntityUIState.Name, null, CharacterNameCanvasRenderer.GetColor()).AddTo(this);
	}
}
