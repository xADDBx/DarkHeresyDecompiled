using System;
using Kingmaker.Code.View.Bridge.Data;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartName : UnitInfoPart
{
	[SerializeField]
	private TextMeshProUGUI m_UnitName;

	[SerializeField]
	private OwlcatMultiSelectable m_AdditionalCombatObjectiveSelectable;

	[SerializeField]
	private Material m_PreciseAttackFontMaterial;

	[SerializeField]
	private Material m_DefaultFontMaterial;

	[SerializeField]
	private TextStyle m_DefaultTextStyle;

	[SerializeField]
	private TextStyle m_AdditionalCombatObjectiveTextStyle;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.Name.Subscribe(SetUnitNameText).AddTo(this);
		base.ViewModel.Data.IsAdditionalCombatObjective.CombineLatest(base.ViewModel.Data.IsNewCombatObjective, (bool _, bool _) => true).DebounceFrame(1).Subscribe(delegate
		{
			UpdateAdditionalCombatObjective();
		})
			.AddTo(this);
	}

	private void UpdateAdditionalCombatObjective()
	{
		SetUnitNameText(base.ViewModel.Data.Name.CurrentValue);
		if (!base.ViewModel.Data.IsAdditionalCombatObjective.CurrentValue)
		{
			m_AdditionalCombatObjectiveSelectable.SetActiveLayer("Default");
			return;
		}
		string activeLayer = (base.ViewModel.Data.IsNewCombatObjective.CurrentValue ? "NewCombatObj" : "CombatObj");
		m_AdditionalCombatObjectiveSelectable.SetActiveLayer(activeLayer);
	}

	private void SetUnitNameText(string unitName)
	{
		Material material = (base.ViewModel.IsPreciseAttack.CurrentValue ? m_PreciseAttackFontMaterial : m_DefaultFontMaterial);
		if (m_UnitName.fontSharedMaterial != material)
		{
			m_UnitName.fontSharedMaterial = material;
			m_UnitName.SetMaterialDirty();
		}
		TextStyle textStyle = (base.ViewModel.Data.IsAdditionalCombatObjective.CurrentValue ? m_AdditionalCombatObjectiveTextStyle : m_DefaultTextStyle);
		try
		{
			string text = "<style=" + textStyle.Style.name + ">" + unitName + "</style>";
			m_UnitName.SetText(text);
			return;
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
		}
		m_UnitName.SetText(unitName);
	}
}
