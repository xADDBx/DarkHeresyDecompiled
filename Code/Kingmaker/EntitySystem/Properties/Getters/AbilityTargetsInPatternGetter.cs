using System;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[ClassInfoBox("Количество подходящих целей в паттерне абилки. В данный момент работает только в триггерах на применение эффекта абилки (урон, скиллчеки, накладывание баффа, etc) и НЕ работает в триггерах на старт/конец каста. Так же не работает для предикшена.")]
[TypeId("ea8e247dab444a78994efcf8bfdd6f2b")]
public sealed class AbilityTargetsInPatternGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return SimpleContextData<AbilityExecutionProcess, AbilityExecutionProcess.Scope>.Current?.Context.TargetsInPatternCount ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of Targets in Ability's Pattern";
	}
}
