using System;
using Kingmaker.Localization;
using Kingmaker.UIDataProvider;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[Obsolete("Ask Alexander Gusev, replaceable with prototypes functionality")]
[TypeId("51210e03e441ea249be955610f84c748")]
public class BlueprintWeaponType : BlueprintScriptableObject, IUIDataProvider
{
	[SerializeField]
	private LocalizedString m_DefaultNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	private Sprite m_Icon;

	public LocalizedString DefaultName => m_DefaultNameText;

	public LocalizedString Description => m_DescriptionText;

	public Sprite Icon => m_Icon;

	string IUIDataProvider.Name => DefaultName;

	string IUIDataProvider.Description => Description;

	Sprite IUIDataProvider.Icon => Icon;

	string IUIDataProvider.NameForAcronym => name;
}
