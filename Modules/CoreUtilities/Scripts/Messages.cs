using UnityEngine;
using System;

namespace HitTrax.CoreUtilities
{
    public interface IConfirm
    {
        Action OnConfirm { get; set; }
    }

    public interface ICancel
    {
        Action OnCancel { get; set; }
    }

    // Basic Message Info
    // Screens and Screen Managers are not bound to this type
    // We can pass whatever message we want
    // That said, Screen Manager, in this current POC, can auto close screens
    // w/ messages that implement IConfirm and ICancel once the function once called

    public struct AppMessageInfo : IConfirm, ICancel
    {
        public string service;
        public string title;
        public string shortDescription;
        public string longDescription;

        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }

        public AppMessageInfo SetService(string s)
        {
            service = s;
            return this;
        }

        // TEMP
        public override string ToString()
        {
            return longDescription;
        }
    }
}
