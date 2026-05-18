using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffView : View<BuffVM>
{
	[Serializable]
	private struct NamedColor
	{
		public string Name;

		public Color Color;
	}

	[SerializeField]
	private BuffView m_BuffView;

	[Space]
	[SerializeField]
	private Graphic m_RankGraphic;

	[SerializeField]
	private Color m_DefaultColor = Color.white;

	[SerializeField]
	private NamedColor[] m_RankColors;

	private IReadOnlyDictionary<string, Color> m_RankColorsByName;

	protected override void OnBind()
	{
		m_BuffView.Bind(base.ViewModel);
		string rankColorName = GetRankColorName();
		Color rankColor = GetRankColor(rankColorName);
		m_RankGraphic.color = rankColor;
	}

	protected override void OnUnbind()
	{
		m_BuffView.Unbind();
	}

	private string GetRankColorName()
	{
		DOTLogic component = base.ViewModel.Buff.Blueprint.GetComponent<DOTLogic>();
		if (component != null)
		{
			return component.Type.ToString().ToLowerInvariant();
		}
		return string.Empty;
	}

	private Color GetRankColor(string colorName)
	{
		if (m_RankColorsByName == null)
		{
			m_RankColorsByName = m_RankColors.ToDictionary((NamedColor c) => c.Name.ToLowerInvariant(), (NamedColor c) => c.Color);
		}
		if (m_RankColorsByName.TryGetValue(colorName, out var value))
		{
			return value;
		}
		return m_DefaultColor;
	}
}
