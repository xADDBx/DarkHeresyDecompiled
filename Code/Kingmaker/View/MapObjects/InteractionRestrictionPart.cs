using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class InteractionRestrictionPart : ViewBasedPart, IInteractionRestriction, IHashable, IOwlPackable<InteractionRestrictionPart>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IsDisabled;

	public virtual SharedStringAsset RestrictedBark { get; }

	public virtual SharedStringAsset AllowedBark { get; }

	public abstract int GetUserPriority(BaseUnitEntity user);

	public abstract bool CheckRestriction(BaseUnitEntity user);

	public virtual void OnDidInteract(BaseUnitEntity user)
	{
	}

	public virtual void OnFailedInteract(BaseUnitEntity user)
	{
	}

	protected virtual string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		return restricted ? ConfigRoot.Instance.LocalizedTexts.LockedContainer : ConfigRoot.Instance.LocalizedTexts.UnlockedContainer;
	}

	public void ShowRestrictionBark(BaseUnitEntity user)
	{
		ShowBarkInternal(user, restricted: true);
	}

	public void ShowSuccessBark(BaseUnitEntity user)
	{
		ShowBarkInternal(user, restricted: false);
	}

	private void ShowBarkInternal(BaseUnitEntity user, bool restricted)
	{
		LocalizedString localizedString = (restricted ? RestrictedBark : AllowedBark)?.String;
		string text = ((localizedString != null) ? ((string)localizedString) : GetDefaultBark(user, restricted));
		if (!string.IsNullOrEmpty(text))
		{
			BarkPlayer.Bark(user, text, VoiceOverType.Bark, user.VoGuid, -1f, null, user);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsDisabled);
		return result;
	}

	public abstract override void Serialize<TFormatter>(TFormatter formatter, SerializerState state);

	public abstract override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state);
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class InteractionRestrictionPart<TSettings> : InteractionRestrictionPart, IHashable, IOwlPackable<InteractionRestrictionPart<TSettings>> where TSettings : class, new()
{
	public TSettings Settings { get; private set; } = new TSettings();


	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
