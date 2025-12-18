using System.Collections;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Effects.GlobalEffects;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Kingmaker.Framework.GlobalEffectSystem;

public sealed class GlobalEffectViewController : MonoBehaviour
{
	public const string SceneName = "[global effects]";

	private readonly Dictionary<BlueprintGlobalEffect, GlobalEffect> _effects = new Dictionary<BlueprintGlobalEffect, GlobalEffect>();

	private readonly Dictionary<BlueprintGlobalEffect, Coroutine> _deactivateCoroutines = new Dictionary<BlueprintGlobalEffect, Coroutine>();

	[SerializeField]
	private int _inactiveEffectDeactivationDelay = 10;

	private Scene Scene => SceneManager.GetSceneByName("[global effects]");

	private void Start()
	{
		if (!Scene.IsValid())
		{
			SceneManager.CreateScene("[global effects]");
		}
	}

	private void LateUpdate()
	{
		Dictionary<BlueprintGlobalEffect, float> value;
		using (CollectionPool<Dictionary<BlueprintGlobalEffect, float>, KeyValuePair<BlueprintGlobalEffect, float>>.Get(out value))
		{
			foreach (BlueprintGlobalEffect key3 in GlobalEffectDirector.Shared.Effects.Keys)
			{
				value.Add(key3, GlobalEffectDirector.Shared.GetWeight(key3));
			}
			BlueprintGlobalEffect key;
			foreach (KeyValuePair<BlueprintGlobalEffect, GlobalEffect> effect in _effects)
			{
				effect.Deconstruct(out key, out var _);
				BlueprintGlobalEffect key2 = key;
				value.TryAdd(key2, 0f);
			}
			foreach (KeyValuePair<BlueprintGlobalEffect, float> item in value)
			{
				item.Deconstruct(out key, out var value3);
				BlueprintGlobalEffect blueprintGlobalEffect = key;
				float num = value3;
				if (!_effects.TryGetValue(blueprintGlobalEffect, out GlobalEffect value4))
				{
					GlobalEffect globalEffect = blueprintGlobalEffect.Prefab.Load();
					if (globalEffect == null)
					{
						continue;
					}
					value4 = (_effects[blueprintGlobalEffect] = Object.Instantiate(globalEffect));
					SceneManager.MoveGameObjectToScene(value4.gameObject, Scene);
					if (num <= 0f)
					{
						value4.gameObject.SetActive(value: false);
					}
				}
				value4.State.Weight = num;
				if (num > 0f)
				{
					if (_deactivateCoroutines.TryGetValue(blueprintGlobalEffect, out Coroutine value5))
					{
						StopCoroutine(value5);
						_deactivateCoroutines.Remove(blueprintGlobalEffect);
					}
					if (!value4.gameObject.activeSelf)
					{
						value4.gameObject.SetActive(value: true);
					}
				}
				else if (!_deactivateCoroutines.ContainsKey(blueprintGlobalEffect))
				{
					_deactivateCoroutines.Add(blueprintGlobalEffect, StartCoroutine(DeactivateEffect(blueprintGlobalEffect)));
				}
			}
		}
	}

	private void OnDestroy()
	{
		foreach (GlobalEffect value in _effects.Values)
		{
			if (value != null)
			{
				Object.Destroy(value.gameObject);
			}
		}
		_effects.Clear();
		SceneManager.UnloadSceneAsync("[global effects]");
	}

	private IEnumerator DeactivateEffect(BlueprintGlobalEffect blueprint)
	{
		yield return new WaitForSeconds(_inactiveEffectDeactivationDelay);
		_deactivateCoroutines.Remove(blueprint);
		_effects.GetValueOrDefault(blueprint).gameObject.SetActive(value: false);
	}

	public GlobalEffect? GetEffect(BlueprintGlobalEffect blueprint)
	{
		return _effects.GetValueOrDefault(blueprint);
	}
}
