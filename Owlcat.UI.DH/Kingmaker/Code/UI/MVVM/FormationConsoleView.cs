using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FormationConsoleView : FormationBaseView
{
	[SerializeField]
	private FormationCharacterConsoleView m_CharacterView;

	[Header("Console")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[Header("Hints")]
	[SerializeField]
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsTopWidget;

	[SerializeField]
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsBottomWidget;

	[SerializeField]
	private ConsoleHint m_LeftHint;

	[SerializeField]
	private ConsoleHint m_RightHint;

	[SerializeField]
	private ConsoleHint m_MoveCharacterFree;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_FormationHintConsole;

	private bool m_MoveFreely;

	private readonly ReactiveProperty<bool> m_IsCustomAndCharacter = new ReactiveProperty<bool>();

	private readonly List<FormationCharacterConsoleView> m_Characters = new List<FormationCharacterConsoleView>();

	private InputLayer m_InputLayer;

	private const float Threshold = 0.5f;

	private FloatConsoleNavigationBehaviour NavigationBehaviour { get; set; }

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
		m_ConsoleHintsTopWidget.Dispose();
		m_ConsoleHintsBottomWidget.Dispose();
		m_MoveFreely = false;
		base.OnUnbind();
		m_Characters.ForEach(WidgetFactory.DisposeWidget);
		m_Characters.Clear();
		TooltipHelper.HideTooltip();
		GamePad.Instance.BaseLayer?.Bind();
	}

	private void CreateInput()
	{
		NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters);
		m_InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Formation"
		});
		m_LeftHint.Bind(m_InputLayer.AddButton(delegate
		{
			UISounds.Instance.PlayHoverSound();
			base.ViewModel.FormationSelector.SelectPrevValidEntity();
		}, 14)).AddTo(this);
		m_RightHint.Bind(m_InputLayer.AddButton(delegate
		{
			UISounds.Instance.PlayHoverSound();
			OnSelectFormation();
		}, 15)).AddTo(this);
		m_MoveCharacterFree.Bind(m_InputLayer.AddButton(delegate
		{
			SetMoveFreely(value: true);
		}, 12, m_IsCustomAndCharacter)).AddTo(this);
		m_MoveCharacterFree.SetLabel(UIStrings.Instance.FormationTexts.MoveCharacterFree);
		m_ConsoleHintsBottomWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow);
		m_ConsoleHintsBottomWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.ResetCurrentFormation();
		}, 11, m_IsCustomAndCharacter, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.FormationTexts.RestoreToDefault);
		m_ConsoleHintsTopWidget.CreateCustomHint(new List<int> { 2, 3 }, m_InputLayer, UIStrings.Instance.FormationTexts.MoveCharacter, m_IsCustomAndCharacter);
		m_ConsoleHintsTopWidget.CreateCustomHint(new List<int> { 0, 1 }, m_InputLayer, UIStrings.Instance.FormationTexts.ChangeCharacter, m_IsCustomAndCharacter);
		m_InputLayer.AddButton(delegate
		{
			SetMoveFreely(value: false);
		}, 12, InputActionEventType.ButtonJustReleased);
		m_InputLayer.AddButton(delegate
		{
			SetMoveFreely(value: false);
		}, 12, InputActionEventType.ButtonLongPressJustReleased);
		m_InputLayer.AddAxis2D(MoveCharacter, 2, 3, m_IsCustomAndCharacter);
		foreach (FormationCharacterVM character in base.ViewModel.Characters)
		{
			FormationCharacterConsoleView widget = WidgetFactory.GetWidget(m_CharacterView);
			widget.transform.SetParent(m_CharacterContainer, worldPositionStays: false);
			widget.Bind(character);
			m_Characters.Add(widget);
		}
		NavigationBehaviour.Focus.Subscribe(delegate
		{
			OnFocusEntity();
		}).AddTo(this);
		UpdateNavigation();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		GamePad.Instance.BaseLayer?.Unbind();
	}

	private void SetMoveFreely(bool value)
	{
		m_MoveFreely = value;
	}

	private void UpdateNavigation()
	{
		NavigationBehaviour.Clear();
		if (base.ViewModel.IsCustomFormation)
		{
			NavigationBehaviour.AddEntities(m_Characters);
			NavigationBehaviour.FocusOnFirstValidEntity();
		}
	}

	private void OnFocusEntity()
	{
		m_IsCustomAndCharacter.Value = base.ViewModel.IsCustomFormation;
	}

	private void MoveCharacter(InputActionEventData data, Vector2 vec)
	{
		FormationCharacterConsoleView formationCharacterConsoleView = NavigationBehaviour.Focus.Value as FormationCharacterConsoleView;
		if (!(formationCharacterConsoleView == null))
		{
			float x = ((Mathf.Abs(vec.x) > 0.5f) ? vec.x : 0f);
			float y = ((Mathf.Abs(vec.y) > 0.5f) ? vec.y : 0f);
			formationCharacterConsoleView.MoveCharacter(new Vector2(x, y), m_MoveFreely);
		}
	}

	public override void OnFormationPresetIndexChanged(int formationPresetIndex)
	{
		base.OnFormationPresetIndexChanged(formationPresetIndex);
		UpdateNavigation();
	}
}
