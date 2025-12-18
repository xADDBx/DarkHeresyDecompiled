using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintClue))]
[TypeId("1f426009f61749ffa826722adbb14f43")]
public class ClueToItemLink : BlueprintComponent
{
	[Serializable]
	public class LinkEntity
	{
		[SerializeField]
		private BpRef<BlueprintItem> m_Item;

		[SerializeField]
		private ConditionsChecker m_Condition;

		public BlueprintItem Item => m_Item;

		public bool IsAvailable => m_Condition.Check();
	}

	[SerializeField]
	private List<LinkEntity> m_Links = new List<LinkEntity>();

	public BlueprintItem GetLastLinkedItem()
	{
		return m_Links.LastOrDefault((LinkEntity l) => l.IsAvailable)?.Item;
	}
}
