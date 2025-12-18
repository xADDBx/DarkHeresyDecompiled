using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioEntityBaseView : SelectionGroupEntityView<NewGamePhaseStoryScenarioEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Elements")]
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private Sprite m_TransparentSprite;

	[SerializeField]
	private Sprite m_DlcAvailableSprite;

	[SerializeField]
	private Sprite m_DlcNotAvailableSprite;

	private PantographConfig PantographConfig { get; set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.Title;
		m_Button.SetActiveLayer(base.ViewModel.IsStoryIsAvailable.CurrentValue ? "Available" : "NotAvailable");
		SetupPantographConfig();
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		WidgetFactory.DisposeWidget(this);
	}

	private void DrawEntities()
	{
		DrawEntitiesImpl();
	}

	protected virtual void DrawEntitiesImpl()
	{
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		if (value)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
			base.ViewModel.SelectMe();
		}
		OnChangeSelectedStateImpl();
	}

	protected virtual void OnChangeSelectedStateImpl()
	{
	}

	private void SetupPantographConfig()
	{
		List<Sprite> icons = new List<Sprite>
		{
			m_TransparentSprite,
			base.ViewModel.IsStoryIsAvailable.CurrentValue ? m_DlcAvailableSprite : m_DlcNotAvailableSprite
		};
		PantographConfig = new PantographConfig(m_Button.transform, base.ViewModel.Title, icons);
	}
}
