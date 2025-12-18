using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBigPortraitView : View<CharInfoBigPortraitVM>
{
	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private CharInfoHitPointsPCView m_HitPointsView;

	[SerializeField]
	private CharInfoExperiencePCView m_Experience;

	public void Awake()
	{
		m_Experience.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_Experience.Bind(base.ViewModel.Experience);
		m_HitPointsView.Or(null)?.Bind(base.ViewModel.HitPoints);
		base.ViewModel.Unit?.Subscribe(delegate
		{
			RefreshView();
		}).AddTo(this);
	}

	protected void RefreshView()
	{
		string unitName = base.ViewModel.UnitName;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != unitName)
		{
			m_NameFieldScrambled.SetText(string.Empty, unitName);
		}
		m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitFull);
	}
}
