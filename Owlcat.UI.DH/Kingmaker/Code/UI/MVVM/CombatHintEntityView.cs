using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatHintEntityView : View<CombatHintEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private GameObject m_TitleContainer;

	[SerializeField]
	private GameObject m_NewIcon;

	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[Header("Values")]
	[SerializeField]
	private string m_NewObjectPrefix;

	protected override void OnBind()
	{
		m_TitleContainer.SetActive(UIConfig.Instance.CombatConfig.DebugFlags.HasFlag(CombatDebugFlags.ShowMapObjectTooltipTitle));
		m_Title.text = base.ViewModel.CombatObjective.Name;
		base.ViewModel.CurrentState.Subscribe(delegate(OvertipState value)
		{
			m_NewIcon.SetActive(value == OvertipState.New);
			string text = ((value == OvertipState.New) ? m_NewObjectPrefix : string.Empty);
			m_Description.text = text + base.ViewModel.CombatObjective.Description;
		}).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
