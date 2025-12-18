using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Components;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class PersonInfoView : View<DetectivePersonInfo>
{
	[Header("Views")]
	[SerializeField]
	private BodyTypeViews m_BodyTypeViews;

	[SerializeField]
	private HeightFenceView m_HeightFenceView;

	[Header("Elements")]
	[SerializeField]
	private Image m_SideFace;

	[SerializeField]
	private GameObject m_EmptyDataContainer;

	[SerializeField]
	private GameObject m_EmptySidePortrait;

	[SerializeField]
	private List<PersonInfoEntity> m_InfoEntities = new List<PersonInfoEntity>();

	[Header("Values")]
	[SerializeField]
	private Sprite m_DefaultIcon;

	protected override void OnBind()
	{
		Sprite sprite = base.ViewModel.IconInProfile.LoadAsset();
		m_SideFace.sprite = sprite ?? m_DefaultIcon;
		m_EmptySidePortrait.Or(null)?.SetActive(sprite == null);
		m_HeightFenceView.Bind(base.ViewModel.Height);
		m_BodyTypeViews.Bind(base.ViewModel.BodyType);
		m_InfoEntities.ForEach(delegate(PersonInfoEntity i)
		{
			i.Bind(base.ViewModel);
		});
		m_EmptyDataContainer.SetActive(!m_InfoEntities.Any((PersonInfoEntity e) => e.gameObject.activeSelf));
	}

	protected override void OnUnbind()
	{
		m_SideFace.sprite = m_DefaultIcon;
		m_HeightFenceView.Unbind();
		m_BodyTypeViews.Unbind();
		m_InfoEntities.ForEach(delegate(PersonInfoEntity i)
		{
			i.Unbind();
		});
	}
}
