using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("Tags: 't|SourceFact' - Triggered buff\n't|TargetUnit' - Unit who acquired buff")]
[TypeId("990500828f6b1c54ca434d39037fb36e")]
public class TutorialTriggerBuffStatusUpdate : TutorialTrigger, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private enum BuffEventType
	{
		Attach,
		Detach
	}

	[SerializeField]
	private BuffEventType m_EventType;

	[SerializeField]
	private bool m_ChooseSpecificBuff = true;

	[SerializeField]
	[ShowIf("m_ChooseSpecificBuff")]
	private BlueprintBuffReference m_Buff;

	[HideIf("m_ChooseSpecificBuff")]
	public SpellDescriptorWrapper TriggerDescriptors;

	[HideIf("m_ChooseSpecificBuff")]
	[InfoBox(Text = "If NeedAllDescriptors is true, only buff that has all listed flags will trigger")]
	public bool NeedAllDescriptors;

	public void HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		if (m_EventType == BuffEventType.Attach)
		{
			TryToTrigger(buff);
		}
	}

	public void HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		if (m_EventType == BuffEventType.Detach)
		{
			TryToTrigger(buff);
		}
	}

	private void TryToTrigger(Buff buff)
	{
	}

	public void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
	}

	public void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
	}
}
