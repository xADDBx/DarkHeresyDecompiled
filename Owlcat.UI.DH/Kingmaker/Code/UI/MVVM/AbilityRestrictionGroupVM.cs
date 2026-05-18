using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityRestrictionGroupVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_ShowConnector;

	private readonly ReactiveProperty<NextConnectorData> m_NextConnectorData;

	public readonly IReadOnlyList<AbilityRestrictionEntry> Entries;

	public readonly MechanicEntity Owner;

	public readonly bool IsPassed;

	public readonly bool ShowLogicalOr;

	public readonly string LogicalOrText;

	public ReadOnlyReactiveProperty<bool> ShowConnector => m_ShowConnector;

	public ReadOnlyReactiveProperty<NextConnectorData> NextConnectorData => m_NextConnectorData;

	public AbilityRestrictionGroupVM(IReadOnlyList<AbilityRestrictionEntry> entries, MechanicEntity owner, bool isLogicalOr)
	{
		Owner = owner;
		ShowLogicalOr = isLogicalOr && entries.Count > 1;
		Entries = entries;
		IsPassed = (isLogicalOr ? entries.Any((AbilityRestrictionEntry e) => e.IsPassed) : entries.All((AbilityRestrictionEntry e) => e.IsPassed));
		LogicalOrText = UIStrings.Instance.Tooltips.LogicalOr;
		m_ShowConnector = new ReactiveProperty<bool>(value: true).AddTo(this);
		m_NextConnectorData = new ReactiveProperty<NextConnectorData>().AddTo(this);
	}

	public void SetPreviousGroup([CanBeNull] AbilityRestrictionGroupVM previousGroupVM)
	{
		m_ShowConnector.Value = !(previousGroupVM?.ShowLogicalOr ?? false);
	}

	public void SetNextGroup([CanBeNull] AbilityRestrictionGroupVM nextGroupVM)
	{
		m_NextConnectorData.Value = new NextConnectorData
		{
			ShowConnector = (!ShowLogicalOr && nextGroupVM != null),
			IsPassed = (nextGroupVM?.IsPassed ?? false)
		};
	}
}
