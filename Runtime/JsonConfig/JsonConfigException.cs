
using System.IO;

namespace Sprimate.JsonConfig
{
    public class JsonConfigException : IOException
    {
        public JsonConfigException() : base() { }
        public JsonConfigException(string message) : base(message) { }
    }
}
