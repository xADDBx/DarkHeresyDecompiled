using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickButtonConsoleView : TooltipBrickButtonView, IConsoleTooltipBrick, IConsoleInputHandler, IMonoBehaviour
{
	[SerializeField]
	private OwlcatMultiButton m_ConsoleButton;

	private SimpleConsoleNavigationEntity m_ButtonEntity;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	public bool IsBinded => base.ViewModel != null;

	public MonoBehaviour MonoBehaviour => this;

	protected override void OnBind()
	{
		m_IsFocused.Value = false;
		m_ConsoleButton.OnFocusAsObservable().Subscribe(delegate(bool value)
		{
			m_IsFocused.Value = value;
		}).AddTo(this);
		m_ButtonEntity = new SimpleConsoleNavigationEntity(m_ConsoleButton);
		m_Text.text = base.ViewModel.Text;
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ButtonEntity.SetFocus(value: false);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_ButtonEntity;
	}

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClick();
		}, 8, m_IsFocused);
		hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Accept).AddTo(this);
		inputBindStruct.AddTo(this);
	}

	public void UpdateTooltipBrick()
	{
	}
}
