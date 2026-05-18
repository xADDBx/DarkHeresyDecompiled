using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Blueprints;

[TypeId("f30b1d10b8dc497180447b3e962d14dc")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintMechanicEntityFact : BlueprintFact, IUIDataProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintMechanicEntityFact>
	{
	}

	[SerializeField]
	[FormerlySerializedAs("m_LocalizedName")]
	[ShowIf("ShowDisplayName")]
	private LocalizedString m_DisplayName;

	[SerializeField]
	[FormerlySerializedAs("m_LocalizedDescription")]
	[ShowIf("ShowDescription")]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	public LocalizedString LocalizedName => m_DisplayName;

	public LocalizedString LocalizedDescription => m_Description;

	public virtual string Name => (base.ComponentsArray.FirstOrDefault((BlueprintComponent p) => p is AddStringToFactName) as AddStringToFactName)?.NewString(m_DisplayName) ?? ((string)m_DisplayName);

	public virtual string Description => GetFullDescription();

	public virtual Sprite Icon => m_Icon;

	public CombatTextSettings CombatTextSettings => base.ComponentsArray.FirstOrDefault((BlueprintComponent p) => p is CombatTextSettings) as CombatTextSettings;

	public string NameForAcronym => name;

	protected virtual bool ShowDisplayName => true;

	protected virtual bool ShowDescription => true;

	protected override Type GetFactType()
	{
		return typeof(UnitFact);
	}

	protected string GetFullDescription()
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionFactBlueprint = this;
			string allAdditionalDescriptions = m_Description;
			base.ComponentsArray.OfType<AddStringToFactDescription>().ForEach(delegate(AddStringToFactDescription c)
			{
				allAdditionalDescriptions = c.NewString(allAdditionalDescriptions);
			});
			return allAdditionalDescriptions;
		}
	}

	public virtual MechanicEntityFact CreateFact([CanBeNull] IEvalContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new MechanicEntityFactBlueprinted(this, parentContext);
	}
}
