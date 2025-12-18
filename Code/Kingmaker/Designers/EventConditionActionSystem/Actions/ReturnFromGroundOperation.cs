using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("862e6465fbfe41e4ad63ce755dbb2c1d")]
public class ReturnFromGroundOperation : GameAction
{
	[SerializeField]
	[CanBeNull]
	private BlueprintAreaEnterPointReference m_AreaEnterPoint;

	[SerializeField]
	private AutoSaveMode m_AutoSaveMode;

	public override string GetCaption()
	{
		return "Return from area to last Sector/System map";
	}

	protected override void RunAction()
	{
	}
}
