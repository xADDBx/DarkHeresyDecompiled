using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Code.Gameplay.Components;

[ComponentName("UI/CombatTextSettings")]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("5bd498b8e10245ae8872913262e3391f")]
public class CombatTextSettings : BlueprintComponent
{
	[InfoBox("HideInUI flag in Bluprint blocks all UI visual, includes Combat textBluprintFeature — by default Combat Text is hidden. Add components if you want to show it in Combat texts. BluprintBuff — by default buff combat texts active Add component when you want to hide it.")]
	public bool HideCombatText = true;

	[HideIf("HideCombatText")]
	public List<CombatTextCase> Cases = new List<CombatTextCase>();

	private Dictionary<(CombatTextTargetType, CombatTextEventType), CombatTextCaseSettings> m_Lookup;

	public CombatTextCaseSettings GetSettings(CombatTextTargetType targetType, CombatTextEventType eventType)
	{
		if (m_Lookup == null)
		{
			m_Lookup = new Dictionary<(CombatTextTargetType, CombatTextEventType), CombatTextCaseSettings>();
			foreach (CombatTextCase @case in Cases)
			{
				foreach (CombatTextTargetType value2 in Enum.GetValues(typeof(CombatTextTargetType)))
				{
					if (!@case.Targets.HasFlag(value2))
					{
						continue;
					}
					foreach (CombatTextEventType value3 in Enum.GetValues(typeof(CombatTextEventType)))
					{
						if (@case.Events.HasFlag(value3))
						{
							m_Lookup[(value2, value3)] = @case.Settings;
						}
					}
				}
			}
		}
		m_Lookup.TryGetValue((targetType, eventType), out var value);
		return value;
	}
}
