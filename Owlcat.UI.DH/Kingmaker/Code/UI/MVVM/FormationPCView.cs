using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class FormationPCView : FormationBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private FormationCharacterPCView m_CharacterView;

	[Header("Buttons")]
	[SerializeField]
	[UsedImplicitly]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	[UsedImplicitly]
	private OwlcatButton m_ResetButton;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_FormationHintPc;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_ResetLabel;

	private readonly List<FormationCharacterPCView> m_Characters = new List<FormationCharacterPCView>();

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ResetButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ResetCurrentFormation();
		}).AddTo(this);
		m_FormationHintPc.text = SetFormationHintText();
		m_ResetLabel.text = UIStrings.Instance.FormationTexts.RestoreToDefault;
		foreach (FormationCharacterVM character in base.ViewModel.Characters)
		{
			FormationCharacterPCView widget = WidgetFactory.GetWidget(m_CharacterView);
			widget.transform.SetParent(m_CharacterContainer, worldPositionStays: false);
			widget.Bind(character);
			m_Characters.Add(widget);
		}
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, delegate
		{
			base.ViewModel.FormationSelector.SelectPrevValidEntity();
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, base.OnSelectFormation).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Characters.ForEach(WidgetFactory.DisposeWidget);
		m_Characters.Clear();
	}

	private string SetFormationHintText()
	{
		return base.ViewModel.IsCustomFormation ? UIStrings.Instance.FormationTexts.FormationPcHint : UIStrings.Instance.FormationTexts.OptimizedFormation;
	}

	public override void OnFormationPresetIndexChanged(int formationPresetIndex)
	{
		base.OnFormationPresetIndexChanged(formationPresetIndex);
		m_ResetButton.Interactable = base.ViewModel.IsCustomFormation;
		m_FormationHintPc.text = SetFormationHintText();
	}
}
