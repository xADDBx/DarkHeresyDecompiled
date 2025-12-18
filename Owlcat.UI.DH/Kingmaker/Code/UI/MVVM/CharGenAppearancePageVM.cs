using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenAppearancePageVM : SelectionGroupEntityVM
{
	public readonly ReadOnlyReactiveProperty<bool> IsInDetailedView;

	private readonly CharGenContext m_CharGenContext;

	private readonly Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM> m_ComponentsByType = new Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM>();

	private CompositeDisposable m_ComponentSubscriptions;

	public readonly string PageLabel;

	public readonly CharGenAppearancePageType PageType;

	public AutoDisposingList<BaseCharGenAppearancePageComponentVM> Components { get; } = new AutoDisposingList<BaseCharGenAppearancePageComponentVM>();


	public CharGenAppearancePageVM(CharGenContext ctx, CharGenAppearancePageType pageType, ReadOnlyReactiveProperty<bool> isInDetailedView)
		: base(allowSwitchOff: false)
	{
		PageType = pageType;
		PageLabel = UIStrings.Instance.CharGen.GetPageLabelByType(pageType);
		m_CharGenContext = ctx;
		IsInDetailedView = isInDetailedView;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}

	public void BeginPageView()
	{
		CreateComponentsIfNeeded();
		foreach (BaseCharGenAppearancePageComponentVM component in Components)
		{
			component.OnBeginView();
		}
	}

	private void Clear()
	{
		m_ComponentsByType.Clear();
		Components.Clear();
		m_ComponentSubscriptions?.Clear();
		m_ComponentSubscriptions = null;
	}

	public void CreateComponentsIfNeeded()
	{
		if (!Components.Any())
		{
			CreateComponents();
		}
	}

	private void CreateComponents()
	{
		Clear();
		m_ComponentSubscriptions = new CompositeDisposable();
		foreach (CharGenAppearancePageComponent components in CharGenAppearancePages.GetComponentsList(PageType))
		{
			BaseCharGenAppearancePageComponentVM component = CharGenAppearanceComponentFactory.GetComponent(components, m_CharGenContext);
			if (component != null)
			{
				Components.Add(component);
				m_ComponentsByType[components] = component;
				component.OnChanged.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(CharGenAppearancePageComponent value)
				{
					OnComponentChanged(value);
				}).AddTo(m_ComponentSubscriptions);
			}
		}
	}

	private void OnComponentChanged(CharGenAppearancePageComponent changedComponent)
	{
		List<CharGenAppearancePageComponent> list = new List<CharGenAppearancePageComponent>();
		switch (changedComponent)
		{
		case CharGenAppearancePageComponent.BodyType:
			list.Add(CharGenAppearancePageComponent.SkinColour);
			break;
		case CharGenAppearancePageComponent.HairType:
			list.Add(CharGenAppearancePageComponent.HairColour);
			break;
		case CharGenAppearancePageComponent.EyebrowType:
			list.Add(CharGenAppearancePageComponent.EyebrowColour);
			break;
		case CharGenAppearancePageComponent.BeardType:
			list.Add(CharGenAppearancePageComponent.BeardColour);
			break;
		case CharGenAppearancePageComponent.Tattoo:
			list.Add(CharGenAppearancePageComponent.TattooColor);
			break;
		}
		UpdateComponents(list);
	}

	public void UpdateComponents()
	{
		UpdateComponents(m_ComponentsByType.Keys);
	}

	public void UpdateComponent(CharGenAppearancePageComponent changedComponent)
	{
		OnComponentChanged(changedComponent);
	}

	private void UpdateComponents(IEnumerable<CharGenAppearancePageComponent> componentTypes)
	{
		foreach (CharGenAppearancePageComponent componentType in componentTypes)
		{
			if (m_ComponentsByType.TryGetValue(componentType, out var value))
			{
				CharGenAppearanceComponentFactory.UpdateComponent(componentType, value, m_CharGenContext);
			}
		}
	}

	protected override void DoSelectMe()
	{
	}
}
