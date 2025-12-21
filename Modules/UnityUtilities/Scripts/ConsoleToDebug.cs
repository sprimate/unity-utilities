using System.IO;
using System.Text;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public class ConsoleToDebug : TextWriter
    {
        public override void WriteLine(string message)
        {
            try
            {
                if (Application.isPlaying)
                {
                    Debug.Log(message);
                }
            }
            catch { }
        }

        public override Encoding Encoding => System.Text.Encoding.UTF8;
    }
}