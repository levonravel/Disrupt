using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class MatchButton : MonoBehaviour
    {
        public NatInfo MatchInfo;
        
        public void Connect()
        {
            Disrupt.Manager.ConnectToMatch(MatchInfo);
        }
    }
}
