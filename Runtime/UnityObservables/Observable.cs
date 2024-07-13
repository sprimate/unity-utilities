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
    public class Observable<T> : Observable, IObservable<T>, IEquatable<Observable<T>> {

        [SerializeField]
        protected T value;

        // When the observables value is changed in the inspector or due to an UNDO operation we can compare
        // it to this variable to see if an event should be fired.
        T prevValue;
        bool prevValueInitialized = false;
        
        /// <summary>
        /// Fires when the value is changed.
        /// </summary>
        public override event Action OnChanged;
        /// <summary>
        /// Fires when the value is changed, but also includes the previous value. The first parameter is the previous value, the second parameter is the new value.
        /// </summary>
        public event Action<T, T> OnChangedValues;

        /// <summary>
        /// Fires when the value is changed, but also includes the previous value. The first parameter is the new value
        /// </summary>
        public event Action<T> OnChangedValue;

        protected override string ValuePropName { get { return "value"; } }

        public Observable() {
        }

        public Observable(T value) {
            this.value = value;
        }

        bool forceSendEvents = false;
        bool skipPreProcessing;
        public virtual T Value {
            get { return value; }
            set {
                T incomingVal = skipPreProcessing ? value : PreProcessSetValue(value);
                if (EqualityComparer<T>.Default.Equals(incomingVal, this.value) && !forceSendEvents) {
                    return;
                }

                forceSendEvents = false;
                prevValue = value;
                prevValueInitialized = true;
                this.value = incomingVal;
                ProcessEvents(prevValue, true);
                
            }
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

            OnChangedValue.SafeInvoke(valToUse);

            if (OnChangedValues != null)
            {
                OnChangedValues(previousValueToUse, valToUse);
            }
        }

        protected virtual T PreProcessSetValue(T incomingVal) => incomingVal;

        public virtual void SetValue(T value, bool _skipPreProcessing = false, bool _forceSendEvents = false) {
            forceSendEvents = _forceSendEvents;
            skipPreProcessing = _skipPreProcessing;
            Value = value;
            skipPreProcessing = false;
        }

        public static explicit operator T(Observable<T> observable) {
            return observable.value;
        }

        public static bool operator ==(Observable<T> left, T right)
        {
            return left.value.Equals(right);
        }

        public static bool operator !=(Observable<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(Observable<T> left, Observable<T> right)
        {
            return left.value.Equals(right.value);
        }

        public static bool operator !=(Observable<T> left, Observable<T> right)
        {
            return !(left == right);
        }

        public override string ToString() {
            return value.ToString();
        }

        public bool Equals(Observable<T> other) {
            if (other == null) {
                return false;
            }
            return other.value.Equals(value);
        }

        public override bool Equals(object other) {
            return other != null
                && other is Observable<T>
                && ((Observable<T>)other).value.Equals(value);
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        protected override void OnBeginGui() {
            prevValue = value;
            prevValueInitialized = true;
        }

        public override void OnValidate() {
            if (prevValueInitialized) {
                var nextValue = value;
                value = prevValue;
                Value = nextValue;
            } else {
                prevValue = value;
                prevValueInitialized = true;
            }
        }
    }
}