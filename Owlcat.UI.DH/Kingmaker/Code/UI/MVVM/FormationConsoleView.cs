using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FormationConsoleView : FormationBaseView
{
	[SerializeField]
	private FormationCharacterConsoleView m_CharacterView;

	[Header("Hints")]
	[SerializeField]
	private HintView m_LeftHint;

	[SerializeField]
	private HintView m_RightHint;

	[SerializeField]
	private HintView m_MoveCharacterFree;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_FormationHintConsole;

	private bool m_MoveFreely;

	private readonly ReactiveProperty<bool> m_IsCustomAndCharacter = new ReactiveProperty<bool>();

	private readonly List<FormationCharacterConsoleView> m_Characters = new List<FormationCharacterConsoleView>();

	private const float Threshold = 0.5f;

	protected override void OnBind()
	{
		m_MoveFreely = false;
		m_IsCustomAndCharacter.Value = base.ViewModel.IsCustomFormation;
		CreateInput();
		m_FormationHintConsole.text = UIStrings.Instance.FormationTexts.OptimizedFormation;
		m_IsCustomAndCharacter.Subscribe(delegate(bool value)
		{
			m_FormationHintConsole.gameObject.SetActive(!value);
		}).AddTo(this);
		base.OnBind();
	}

	protected override void OnUnbind()
	{
		m_MoveFreely = false;
		base.OnUnbind();
		m_Characters.ForEach(WidgetFactory.DisposeWidget);
		m_Characters.Clear();
		TooltipHelper.HideTooltip();
	}

	private void CreateInput()
	{
	}

	private void SetMoveFreely(bool value)
	{
		m_MoveFreely = value;
	}
}
