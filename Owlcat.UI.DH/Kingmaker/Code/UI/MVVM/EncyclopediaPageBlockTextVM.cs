using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockTextVM : EncyclopediaPageBlockVM
{
	public string Text
	{
		get
		{
			object obj = (m_Block as BlueprintEncyclopediaBlockText)?.GetText();
			if (obj == null)
			{
				EncyclopediaEntryBlock obj2 = m_Block as EncyclopediaEntryBlock;
				if (obj2 == null)
				{
					return null;
				}
				obj = obj2.GetDescription().Text;
			}
			return (string)obj;
		}
	}

	public EncyclopediaPageBlockTextVM(BlueprintEncyclopediaBlockText block)
		: base(block)
	{
	}

	public EncyclopediaPageBlockTextVM(EncyclopediaEntryBlock block)
		: base(block)
	{
	}
}
