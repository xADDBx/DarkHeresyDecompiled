using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Code.View.UI.UIUtilities;

public struct StatBonusEntry
{
	public int Bonus;

	public ModifierType Type;

	[CanBeNull]
	public string Source;

	public ModifierDescriptor Descriptor;

	public static int Compare(StatBonusEntry x, StatBonusEntry y)
	{
		int num = x.Type.CompareTo(y.Type);
		if (num == 0)
		{
			return ModifierDescriptorComparer.Instance.Compare(x.Descriptor, y.Descriptor);
		}
		return num;
	}

	public string GetStatString()
	{
		return string.Format(UIStrings.Instance.Tooltips.GetModifierFormat(Type), UtilityText.FormatModifierValue(Bonus, Type));
	}
}
