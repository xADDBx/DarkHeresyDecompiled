using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyTutorialPartBaseView : View<NetLobbyTutorialPartVM>
{
	[Header("Tutorials")]
	[SerializeField]
	private WidgetList m_WidgetListTutorialBlocks;

	[SerializeField]
	private NetLobbyTutorialBlockView m_NetLobbyTutorialBlockViewPrefab;

	[SerializeField]
	private FadeAnimator m_PartFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_TutorialPartLabel;

	private int m_ShowedBlock;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_PartFadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_ShowedBlock = 0;
		m_PartFadeAnimator.AppearAnimation();
		m_TutorialPartLabel.text = UIStrings.Instance.NetLobbyTexts.HowToPlay;
		DrawEntities();
		ShowBlock();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_ShowedBlock = 0;
	}

	private void DrawEntities()
	{
		if (!base.ViewModel.TutorialBlocksVMs.Any())
		{
			base.ViewModel.OnClose();
		}
		m_WidgetListTutorialBlocks.Clear();
		m_WidgetListTutorialBlocks.DrawEntries(base.ViewModel.TutorialBlocksVMs, m_NetLobbyTutorialBlockViewPrefab);
		m_WidgetListTutorialBlocks.Entries.ForEach(delegate(IBindable b)
		{
			(b as NetLobbyTutorialBlockView).Or(null)?.SetRightArrowVisible(m_WidgetListTutorialBlocks.Entries.LastOrDefault() != b);
		});
	}

	public void ShowBlock()
	{
		if (m_ShowedBlock >= m_WidgetListTutorialBlocks.Entries.Count)
		{
			m_PartFadeAnimator.DisappearAnimation(delegate
			{
				base.ViewModel.OnClose();
			});
		}
		else
		{
			if (m_ShowedBlock < 0 || m_ShowedBlock >= m_WidgetListTutorialBlocks.Entries.Count)
			{
				return;
			}
			if (!base.ViewModel.WithBlocksAnimation)
			{
				m_WidgetListTutorialBlocks.Entries.ForEach(delegate(IBindable b)
				{
					(b as NetLobbyTutorialBlockView).Or(null)?.ShowAnimation();
				});
				m_ShowedBlock = m_WidgetListTutorialBlocks.Entries.Count;
			}
			else
			{
				(m_WidgetListTutorialBlocks.Entries[m_ShowedBlock] as NetLobbyTutorialBlockView).Or(null)?.ShowAnimation(withAnimation: true);
				m_ShowedBlock++;
			}
		}
	}
}
