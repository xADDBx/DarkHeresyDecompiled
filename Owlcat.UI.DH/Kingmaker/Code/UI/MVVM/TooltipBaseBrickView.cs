using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TooltipBaseBrickView<T> : View<T> where T : TooltipBaseBrickVM
{
	protected float m_FontMultiplier;

	protected override void OnBind()
	{
		m_FontMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
	}
}
