using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Getters;

[ClassInfoBox("Medium = 0, Large = 1, Huge = 2, Gargantuan = 3, Colossal = 4")]
[TypeId("571bda39fcd97c741ae37d36c4688bad")]
public sealed class UnitSizeGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Size of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.Size switch
		{
			Size.Medium => 0, 
			Size.Large => 1, 
			Size.Huge => 2, 
			Size.Gargantuan => 3, 
			Size.Colossal => 4, 
			_ => 0, 
		};
	}
}
