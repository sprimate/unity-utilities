using System;
using System.Collections.Generic;

namespace UnityObservables {

    /**
     * A convenient way to bind a single event handler to multiple observables at once. If any of the observables are
     * changed then the action will be invoked.
     */
    public class ObsEffect : IObservable, IDisposable {

        public static ObsEffect Create(params IObservable[] obs) => Create(null, obs, false);
        public static ObsEffect Create(Action action, params IObservable[] obs) => Create(action, obs, true);
        public static ObsEffect CreateNoFireImmediate(Action action, params IObservable[] obs) => Create(action, obs, false);
        public static ObsEffect Create(Action action, IEnumerable<IObservable> obs, bool fireImmediate = true) {
            var instance = new ObsEffect(action, obs);
            if (fireImmediate) {
                action?.Invoke();
            }
            return instance;
        }

        public event Action OnChanged;

        List<IObservable> observables = new List<IObservable>();
        Action action;

        ObsEffect() { }

        ObsEffect(Action action, IEnumerable<IObservable> dependencies) {
            foreach (var o in dependencies) {
                observables.Add(o);
                o.OnChanged += FireEvent;
            }
            this.action = action;
        }

        public void Dispose() {
            foreach (var o in observables) {
                o.OnChanged -= FireEvent;
            }
        }

        void FireEvent() {
            action?.Invoke();
            OnChanged?.Invoke();
        }
    }

}