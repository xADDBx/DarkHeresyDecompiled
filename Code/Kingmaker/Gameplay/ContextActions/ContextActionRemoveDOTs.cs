using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("573bcd343ec4499cb0839b0ad585af35")]
public class ContextActionRemoveDOTs : ContextAction
{
	public bool RemoveAll = true;

	[SerializeField]
	[HideIf("RemoveAll")]
	private ContextValue m_Amount;

	public override string GetCaption()
	{
		return "Remove " + (RemoveAll ? "all DOTS" : $"{m_Amount} DOTs");
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		if (RemoveAll)
		{
			entity.Facts.RemoveAll((Buff i) => i.Blueprint.GetComponent<DOTLogic>() != null);
			return;
		}
		entity.Facts.GetAll((Buff i) => i.Blueprint.GetComponent<DOTLogic>() != null).ToPooledList(out var list);
		int count = m_Amount.Calculate(base.Context);
		foreach (Buff item in list)
		{
			item.RemoveRank(count);
		}
	}
}
