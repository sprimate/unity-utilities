using System;

namespace HitTrax.CoreUtilities
{
    /// <summary>
    /// In context, returning a "none" type will return an empty Safe of T
    /// </summary>
    public struct Nothing { }

    public interface ISafe
    {
        bool HasValue { get; }
    }

    /// <summary>
    /// Type used to reduce the likelihood of null reference exceptions
    /// </summary>
    public struct Safe<T> : ISafe
    {
        internal T _object;
        private bool _isObjectCached;

        public bool HasValue => _object != null && _isObjectCached;

        public Safe(T obj)
        {
            _object = obj;
            _isObjectCached = true;
        }

        private void SetValue(T obj)
        {
            _object = obj;
            _isObjectCached = true;
        }


        public Safe<T> IfSome(Action<T> action)
        {
            if (!HasValue)
            {
                return this;
            }

            action?.Invoke(_object);
            return this;
        }

        public Safe<T> IfNone(Action action)
        {
            if (!HasValue)
            {
                action?.Invoke();
            }

            return this;
        }

        public Safe<T> LogIfNone(string log)
        {
            IfNone(() => Logger.Log(log));
            return this;
        }

        public Safe<T> LogErrorIfNone(string log)
        {
            IfNone(() => Logger.LogError(log));
            return this;
        }

        public Safe<T> Try(Action<T> ifSome, Action ifNone)
        {
            if (HasValue)
            {
                return IfSome(ifSome);
            }
            else
            {
                ifNone?.Invoke();
                return this;
            }
        }

        public Safe<T> Try(Action<T> ifSome)
        {
            if (HasValue)
            {
                return IfSome(ifSome);
            }

            // This basically returns None/Nothing
            return new Safe<T>();
        }

        public Safe<T> SetIfNone(ref T pointer, T obj)
        {
            if (!HasValue || pointer != null)
            {
                return this;
            }

            SetValue(obj);
            pointer = _object;
            return this;
        }

        public Safe<TCast> As<TCast>() where TCast : class => new Safe<TCast>(_object as TCast);

        public static implicit operator Safe<T>(T reference) => new Safe<T>(reference);
        
        public static implicit operator Safe<T>(Nothing empty) => new Safe<T>();

        public override string ToString()
        {
            return $"Safe <{base.ToString()}>";
        }
    }

    public static class SafeFunctions
    {
        /// <summary>
        /// Store a nothing value instead of constantly creating a "new" one whenre calling "None"
        /// </summary>
        private static Nothing _none = new Nothing();

        /// <summary>
        /// Nothing is just that, an empty struct. None is basically by Safe as a new Safe<T>() which is an empty safe object, hence None.
        /// </summary>

        public static Nothing None => _none;

        /// <summary>
        /// A convenience function for taking an unboxed value and wrapping it in "Safe"
        /// </summary>
        public static Safe<T> Safe<T>(this T self) => new Safe<T>(self);

        /// <summary>
        /// Allows you to perform an opperation to the object if it is "Safe". It returns itself.
        /// </summary>
        public static Safe<T> IfSome<T>(this T self, Action<T> action) => Safe(self).IfSome(action);

        /// <summary>
        /// Allows you to perform an operation if the object does not contain a value. It returns itself which will be None.
        /// </summary>
        public static Safe<T> IfNone<T>(this T self, Action action) => Safe(self).IfNone(action);

        /// <summary>
        /// Allows you to perform an opperation if the object is none but also forces you to return a value of the same type which will be of Safe<T>
        /// This is a way of, say, adding a default value to an object that may be Empty
        /// </summary>
        public static Safe<T> IfNone<T>(this Safe<T> self, Func<T> func) => self.HasValue ? self : func().Safe();

        /// <summary>
        /// This is used if you're certain you just want to extract the value out of Safety.
        /// This forces you to return a value in the case that its None, ensuring relative Safety.
        /// Only a person of evil intentions would decide to return "null"
        /// </summary>
        public static T Unbox<T>(this Safe<T> self, Func<T> ifNone) => self.HasValue ? self._object : ifNone();

        /// <summary>
        /// This is used if you're certain you just want to extract the value out of Safety.
        /// This can return null, but the "Safe" type tells the developer to handle it.
        /// This allows them to handle nulls via (?) operators or traditional conditionals.
        /// (Useful for early returns)
        /// </summary>
        public static T UnboxRaw<T>(this Safe<T> self) => self.HasValue ? self._object : default;

        /// <summary>
        /// Renamed from Select. SelectOut makes it clear that you are taking your value and explicitly lifting it out of Safe territory.
        /// This is like a more feature rich version of Unbox where you can return out a different value type and handle both Some and None values
        /// </summary>
        public static R SelectOut<T, R>(this Safe<T> self, Func<T, R> ifSome, Func<R> ifNone) => self.HasValue ? ifSome(self._object) : ifNone();

        /// <summary>
        /// This is a way to force a value on a variable that may be null.
        /// NOTE: I would actually like to deprecate this.
        /// </summary>
        public static Safe<T> SetIfNone<T>(this T self, ref T pointer, T obj) => self.Safe().SetIfNone(ref pointer, obj);

        /// <summary>
        /// Takes an unboxed value and does a null check on it, taking two functions to handle
        /// both some (not null) and none (null) possiblities
        /// </summary>
        public static R CheckNull<T, R>(this T self, Func<T, R> ifSome, Func<R> ifNone) where T : class => self != null ? ifSome(self) : ifNone();

        /// <summary>
        /// This takes an unboxed value T, handles "ifSome" and returns Safe<R>
        /// </summary>
        public static Safe<R> UnboxedSelect<T, R>(this T self, Func<T, R> ifSome) where T : class => self.CheckNull(ifSome, () => default);

        /// <summary>
        /// Performs "Select" on a Safe object, hanlding both Some and None values.
        /// This will return Safe<R>, this allows you to perform many opperations within Safe.
        /// It also prevents you from accidentally returning back Safe(Safe<T>())
        /// </summary>
        public static Safe<R> Select<T, R>(this Safe<T> self, Func<T, Safe<R>> ifSome, Func<R> ifNone) => self.HasValue ? ifSome(self._object) : ifNone();

        /// <summary>
        /// Performs the Select opperation but rather than being forced to handle "None" it will autoumatically return an None value back.
        /// This is good for preventing you from constantly writing redundant code.
        /// </summary>
        public static Safe<R> Select<T, R>(this Safe<T> self, Func<T, R> ifSome) => self.HasValue ? ifSome(self._object) : None;

        /// <summary>
        /// Performs the Select opperation but rather than being forced to handle "None" it will autoumatically return an None value back.
        /// This is good for preventing you from constantly writing redundant code.
        /// This handles what may happen when you pass in a Safe value but you don't want to accidentally return Safe(Safe<R>))
        /// </summary>
        public static Safe<R> Select<T, R>(this Safe<T> self, Func<T, Safe<R>> ifSome) => self.HasValue ? ifSome(self._object) : None;

        /// <summary>
        /// Check to confirm that a group of Safe items have valeus
        /// </summary>
        public static bool AllHaveValues(params ISafe[] items)
        {
            foreach (var item in items)
            {
                if (!item.HasValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}