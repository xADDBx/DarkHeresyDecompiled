using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("6ab3899b2cc347f9ad7ab46d6b3fc355")]
public class ContextConditionIsPreview : ContextCondition
{
	[SerializeField]
	private bool m_IsOnlyChargen;

	protected override string GetConditionCaption()
	{
		return string.Concat($"Check if target is preview, only chargen : {m_IsOnlyChargen}");
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity is BaseUnitEntity { IsPreviewUnit: not false })
		{
			return !m_IsOnlyChargen;
		}
		return false;
	}
}
