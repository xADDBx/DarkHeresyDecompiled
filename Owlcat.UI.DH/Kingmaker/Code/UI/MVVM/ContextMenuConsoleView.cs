using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuConsoleView : ContextMenuView
{
	private ReadOnlyReactiveProperty<ContextMenuEntityConsoleView> m_CurrentEntity;

	public static readonly string InputLayerContextName = "ContextMenu";

	protected override void OnBind()
	{
		base.OnBind();
		CreateInputs();
	}

	private void CreateInputs()
	{
	}
}
