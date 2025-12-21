using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace HitTrax.UnityUtilities
{

	// (Anthony) I'd like to make these more configurable someday
	// With either a SO or JSON for defining actions
	public class KeyCombos
	{
		public HashSet<int> keys;

		public KeyCombos(params int[] keys)
		{
			this.keys = new HashSet<int>(keys);
		}

		public KeyCombos(params KeyCode[] keys)
		{
			this.keys = new HashSet<int>(keys.Select(k => (int)k));
		}

		public bool Contains(int key) => keys.Contains(key);
		public bool Contains(KeyCode key) => Contains((int)key);
		public bool Contains(params KeyCode[] keys) => keys.All(key => Contains(key));

	}
	 
	public class KeyBinding
	{
		private KeyCombos _keyCombos;
		private Action _action;
		private Func<bool> _requirment;

		public KeyBinding(KeyCombos keyCombos, Action action)
		{
			_keyCombos = keyCombos;
			_action = action;
		}

		public KeyBinding(KeyCombos keyCombos, Action action, Func<bool> requirement)
		{
			_keyCombos = keyCombos;
			_action = action;
			_requirment = requirement;
		}

		public bool TryCallback(HashSet<int> keysDown, int keyPressed)
		{
			foreach (var key in _keyCombos.keys)
			{
				if (key != keyPressed && !keysDown.Contains(key))
				{
					return false;
				}
			}

			if (_requirment != null && !_requirment.Invoke())
			{
				return false;
			}

			_action?.Invoke();
			return true;
		}

	}

	// (Anthony) TODO: Like with the DynamicLineManager and UnityAsyncOpperations,
	// I'd like to be able to create categories of keybindings

	public static class KeyboardController
	{
		const int MAX_KEYCODE = 319;

		private static HashSet<int> _keysDown = new();
		private static List<KeyBinding> _keyBindings = new();
		private static bool _initialized = false;

		public static KeyBinding AddKeyBinding(Action action, params KeyCode[] keys)
		{
			TryInit();
			var binding = new KeyBinding(new KeyCombos(keys), action);
			_keyBindings.Add(binding);
			return binding;
		}

		public static KeyBinding AddKeyBinding(Action action, Func<bool> requirement, params KeyCode[] keys)
		{
			TryInit();
			var binding = new KeyBinding(new KeyCombos(keys), action, requirement);
			_keyBindings.Add(binding);
			return binding;
		}

		public static void RemoveAllKeyBindings(this IEnumerable<KeyBinding> keyBindings)
		{
			foreach (var binding in keyBindings)
			{
				RemoveKeyBinding(binding);
			}
		}

		public static void RemoveKeyBinding(KeyBinding keyBinding)
		{
			if (keyBinding == null)
			{
				return;
			}
			_keyBindings.Remove(keyBinding);
		}

		private static void TryInit()
		{
			if (_initialized)
			{
				return;
			}

			UnityAsyncOperations.AddUpdateListener(OnUpdate);

			_initialized = true;
		}

		// Update is called once per frame
		private static void OnUpdate(float delta)
		{
			TestKeysStatus();
		}

		private static void TestKeysStatus()
		{
			for (int i = 0; i < MAX_KEYCODE; i++)
			{
				TestKeyStatus(i);
			}
		}

		private static void TestKeyStatus(int key)
		{
			if (Input.GetKeyDown((KeyCode)key))
			{
				TryActivateKeyBinding(key);
				_keysDown.Add(key);
			}

			if (Input.GetKeyUp((KeyCode)key))
			{
				_keysDown.Remove(key);
			}
		}

		private static void TryActivateKeyBinding(int key)
		{
			// Prevent Collection modification issues
			var bindings = _keyBindings.ToList();

			foreach (KeyBinding binding in bindings)
			{
				binding.TryCallback(_keysDown, key);
			}
		}
	}
}