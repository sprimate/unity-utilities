

namespace Sprimate.JsonConfig
{
    public class JsonConfigLoadException : JsonConfigException
    {
        public enum Type { Malformed, Access, Nonexistent };
        public Type ExceptionType { get; private set; }
        public JsonConfigLoadException(Type type) : base()
        {
            ExceptionType = type;
        }

        public JsonConfigLoadException(Type type, string message) : base(message)
        {
            ExceptionType = type;
        }
    }
}
