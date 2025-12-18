using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockVM : ViewModel
{
	protected readonly IBlock m_Block;

	public readonly float FontMultiplier = FontSizeMultiplier;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public EncyclopediaPageBlockVM(IBlock block)
	{
		m_Block = block;
	}
}
