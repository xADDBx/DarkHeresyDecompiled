using Kingmaker.Blueprints.Root;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartDescription : UnitInfoPart
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private GameObject m_NewIcon;

	[SerializeField]
	private string m_NewDescriptionPrefix;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.Description.Subscribe(delegate
		{
			UpdateDescription();
		}).AddTo(this);
		base.ViewModel.Data.IsNewCombatObjective.Subscribe(delegate
		{
			UpdateDescription();
		}).AddTo(this);
	}

	private void UpdateDescription()
	{
		string currentValue = base.ViewModel.Data.Description.CurrentValue;
		bool flag = UIConfig.Instance.CombatConfig.DebugFlags.HasFlag(CombatDebugFlags.ShowNewIconAtDescription);
		bool flag2 = base.ViewModel.Data.IsNewCombatObjective.CurrentValue && flag;
		base.gameObject.SetActive(!string.IsNullOrEmpty(currentValue));
		string text = (flag2 ? m_NewDescriptionPrefix : string.Empty);
		text += currentValue;
		m_NewIcon.SetActive(flag2);
		m_Description.text = text;
	}
}
