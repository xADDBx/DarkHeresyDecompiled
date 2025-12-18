using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("f809d41697e94786a9733f2379d9fff2")]
public class RevealWarpRoute : GameAction
{
	[SerializeReference]
	public MechanicEntityEvaluator m_System1;

	[SerializeReference]
	public MechanicEntityEvaluator m_System2;

	public override string GetCaption()
	{
		return $"Reveal warp route {m_System1} <-> {m_System2}";
	}

	protected override void RunAction()
	{
	}
}
