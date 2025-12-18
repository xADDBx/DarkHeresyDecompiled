using Kingmaker.Blueprints.Credits;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TitlesMultiColumnPage : CreditsTwoColumnsPage
{
	[SerializeField]
	private CreditsCompanyElement m_CompanyPrefab;

	public override void Append(string row, BlueprintCreditsGroup group)
	{
		base.Append(row, group);
		string text = PageGenerator.ReadCompany(row);
		if (!string.IsNullOrEmpty(text))
		{
			CreditsCompanyElement instance = m_CompanyPrefab.GetInstance<CreditsCompanyElement>();
			instance.Initialize(text, this);
			m_Rows.Add(instance);
		}
	}
}
