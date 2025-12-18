using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsView<TBoolEntity> : View<CharacterVisualSettingsVM>, IInitializable where TBoolEntity : CharacterVisualSettingsEntityView
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	[Header("Main")]
	[SerializeField]
	protected TBoolEntity m_ClothEntityView;

	[SerializeField]
	protected TBoolEntity m_HelmetEntityView;

	[SerializeField]
	protected TBoolEntity m_BackpackEntityView;

	[SerializeField]
	protected TBoolEntity m_HelmetAboveAllEntityView;

	[Header("Color")]
	[SerializeField]
	protected TextureSelectorPagedView m_OutfitMainColorSelectorView;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_OutfitMainColorSelectorView.Initialize();
		m_HelmetEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowHelmet);
		m_BackpackEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowBackpack);
	}

	protected override void OnBind()
	{
		m_Header.text = UIStrings.Instance.CharacterSheet.VisualSettingsTitle;
		m_FadeAnimator.AppearAnimation();
		m_OutfitMainColorSelectorView.Bind(base.ViewModel.OutfitMainColorSelector);
		m_HelmetEntityView.Or(null)?.Bind(base.ViewModel.Helmet);
		m_BackpackEntityView.Or(null)?.Bind(base.ViewModel.Backpack);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
	}
}
