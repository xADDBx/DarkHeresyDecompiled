using System;
using Kingmaker.UI.Common;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[Obsolete]
[TypeId("b2f7c40becfa4e79af7d701025025429")]
public class BlueprintCargoRoot : BlueprintScriptableObject
{
	[Serializable]
	public class CargoTemplate
	{
		[SerializeField]
		private ItemsItemOrigin m_ItemOrigin;

		[SerializeField]
		[ValidateNotNull]
		private BlueprintCargoReference m_Template;

		[SerializeField]
		private int m_ReputationPointsCost;

		public ItemsItemOrigin ItemOrigin => m_ItemOrigin;

		public BlueprintCargo Template => m_Template?.Get();

		public int ReputationPointsCost => m_ReputationPointsCost;
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintCargoRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private CargoTemplate[] m_CargoTemplates;

	[SerializeField]
	private int m_MaxFilledVolumePercentToAddItem = 100;
}
