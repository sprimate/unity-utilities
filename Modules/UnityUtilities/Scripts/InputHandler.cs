using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input handler used to allow for subscribing and unsubscribing key and mouse events
/// NOTE: Building features out only as needed
/// </summary>

namespace HitTrax.UnityUtilities
{
    // (Anthony) I pulled this from legacy but I think KeyboardController is a more feature rich version of this
    // At least as it relates to key input, so I probably can probably merge the mouse input listeners from this
    // remove the keyboard stuff and keep the keyboard stuff from keyboard controller
    
    public static class InputHandler
    {

        const int LEFT = 0;
        const int RIGHT = 1;
        const int MIDDLE = 2;

        public static Vector3 MouseDelta => Input.mousePositionDelta;
        private static Dictionary<KeyCode, bool> _inputStatus = new();
        private static Dictionary<KeyCode, List<Action>> _keyPressedListeners = new();
        private static Dictionary<KeyCode, List<Action>> _keyReleasedListeners = new();
        private static List<Action>[] _releaseListeners = new List<Action>[] { new List<Action>(), new List<Action>(), new List<Action>() };

        private static bool[] _buttonStatus = new bool[] { false, false, false };

        static bool _initialized = false;

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
        static void OnUpdate(float delta)
        {
            UpdateButtonStatus();
        }

        private static void UpdateButtonStatus()
        {
            for (int i = 0; i < _releaseListeners.Length; i++)
            {
                UpdateMouseButtonStatus(i, _releaseListeners[i]);
            }
        }

        private static void UpdateMouseButtonStatus(int buttonIndex, List<Action> listeners)
        {
            if (buttonIndex < 0 || buttonIndex > _buttonStatus.Length - 1)
            {
                return;
            }

            if (!_buttonStatus[buttonIndex] && Input.GetMouseButton(buttonIndex))
            {
                RaiseMouseDown(listeners);
            }

            _buttonStatus[buttonIndex] = Input.GetMouseButton(buttonIndex);
        }
        private static void RaiseMouseDown(List<Action> listeners) => listeners.ForEach(act => act());
        public static void AddMouseButtonListener(int button, Action action)
        {
            TryInit();
            _releaseListeners[button].Add(action);
        }
        public static void RemoveMouseButtonListener(int button, Action action) => _releaseListeners[button].Remove(action);

        public static void AddKeyPressedListener(KeyCode key, Action action)
        {
            TryInit();
            if (_keyPressedListeners == null)
            {
                _keyPressedListeners = new Dictionary<KeyCode, List<Action>>();
            }

            List<Action> listeners;

            if (_keyPressedListeners.TryGetValue(key, out listeners))
            {
                listeners.Add(action);
            }
            else
            {
                listeners = new List<Action>();
                _keyPressedListeners.Add(key, listeners);
                listeners.Add(action);
            }
        }

        public static void RemoveAllMouseListeners(int button) => _releaseListeners[button].Clear();

    }
}