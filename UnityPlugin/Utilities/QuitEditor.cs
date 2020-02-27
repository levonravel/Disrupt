using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class QuitEditor
    {
        static void Quit()
        {
            Disrupt.Client.Dispose();
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.quitting += Quit;
        }
    }
}
