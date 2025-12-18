using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootMainPageConsoleView : StatCheckLootMainPageBaseView<StatCheckLootUnitCardConsoleView>
{
	[Header("Input")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ConsoleHintsWidget.Dispose();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnClose();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right).AddTo(this);
	}
}
