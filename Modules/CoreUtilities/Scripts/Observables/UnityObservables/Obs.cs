using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityObservables {

    public interface IObservable<T> : IObservable {
        event Action<T, T> OnChangedValues;
        T Value { get; }
    }

    [Serializable]
    public class Obs<T> : Obs, IObservable<T>, IEquatable<Obs<T>> {

        [SerializeField]
        protected T _val;

        // When the observables value is changed in the inspector or due to an UNDO operation we can compare
        // it to this variable to see if an event should be fired.
        T _prevValue;
        bool _prevValueInitialized = false;
        
        /// <summary>
        /// Fires when the value is changed (but does not include the new value)
        /// </summary>
        public override event Action OnChanged;
        /// <summary>
        /// Fires when the value is changed, but also includes the previous value. The first parameter is the previous value, the second parameter is the new value.
        /// </summary>
        public event Action<T, T> OnChangedValues;

        /// <summary>
        /// Fires when the value is changed
        /// </summary>
        public event Action<T> OnChangedValue;
        private bool _forceSendEvents = false;
        private bool _skipPreProcessing;
        protected override string ValuePropName { get { return "value"; } }

        public Obs() {
        }

        public Obs(T value) {
            this._val = value;
        }

        
        public virtual T Value {
            get { return _val; }
            set {
                T incomingVal = _skipPreProcessing ? value : PreProcessSetValue(value);
                if (EqualityComparer<T>.Default.Equals(incomingVal, this._val) && !_forceSendEvents) {
                    return;
                }

                _forceSendEvents = false;
                _prevValue = this._val;
                _prevValueInitialized = true;
                this._val = incomingVal;
                ProcessEvents(_prevValue, true);
                
            }
        }
        public T Get() => Value;
        public void Set(T value)
        {
            SetValue(value);
        }

        public virtual void SetValue(T value, bool _skipPreProcessing = false, bool _forceSendEvents = false)
        {
            this._forceSendEvents = _forceSendEvents;
            this._skipPreProcessing = _skipPreProcessing;
            Value = value;
            this._skipPreProcessing = false;
        }

        protected void ProcessEvents(T previousValueToUse, bool forceSend)
        {
            var valToUse = Value;
            if (!forceSend && EqualityComparer<T>.Default.Equals(previousValueToUse, valToUse))
            {
                return;
            }

            if (OnChanged != null)
            {
                OnChanged();
            }

            OnChangedValue?.Invoke(valToUse);

            if (OnChangedValues != null)
            {
                OnChangedValues(previousValueToUse, valToUse);
            }
        }

        protected virtual T PreProcessSetValue(T incomingVal) => incomingVal;

        public static explicit operator T(Obs<T> observable) {
            return observable._val;
        }

        public static bool operator ==(Obs<T> left, T right)
        {
            return left._val.Equals(right);
        }

        public static bool operator !=(Obs<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(Obs<T> left, Obs<T> right)
        {
            return left._val.Equals(right._val);
        }

        public static bool operator !=(Obs<T> left, Obs<T> right)
        {
            return !(left == right);
        }
        // (Priyal)
        //should only EXPLICITLY set the value, so the callback/observer pattern works. Can't overload = for assignment on existing types
        /*public static implicit operator Observable<T>(T value) 
        {
            return new Observable<T>(value);
        }*/

        public override string ToString() {
            return _val.ToString();
        }

        public bool Equals(Obs<T> other) {
            if (other == null) {
                return false;
            }
            return other._val.Equals(_val);
        }

        public override bool Equals(object other) {
            return other != null
                && other is Obs<T>
                && ((Obs<T>)other)._val.Equals(_val);
        }

        public override int GetHashCode() {
            return _val.GetHashCode();
        }

        protected override void OnBeginGui() {
            _prevValue = _val;
            _prevValueInitialized = true;
        }

        public override void OnValidate() {
            if (_prevValueInitialized) {
                var nextValue = _val;
                _val = _prevValue;
                Value = nextValue;
            } else {
                _prevValue = _val;
                _prevValueInitialized = true;
            }
        }
    }
}