using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RavelTek.Disrupt
{
    public class MatchUpdater : MonoBehaviour
    {
        public Transform MatchHolder;
        public GameObject Button;
        public float RefreshRate;
        Coroutine _matchFindRoutine;
        public void Start()
        {
            Disrupt.Client.OnHostList += Events_OnHostList;
            Disrupt.Client.OnConnected += HandleConnected;
            _matchFindRoutine = StartCoroutine(RefreshMatches());
        }

        void HandleConnected(Peer peer)
        {
            if(_matchFindRoutine != null)
                StopCoroutine(_matchFindRoutine);
        }

        IEnumerator RefreshMatches()
        {
            if (Disrupt.Client.IsServer) yield break;
            while (true)
            {
                for (int i = MatchHolder.childCount - 1; i >= 0; i--)
                {
                    Destroy(MatchHolder.GetChild(i).gameObject);
                }
                Disrupt.Manager.FindWanMatches();
                yield return new WaitForSeconds(RefreshRate);
            }
        }
        private void Events_OnHostList(NatInfo[] hosts)
        {
            if (hosts == null) return;
            foreach (var i in hosts)
            {
                var instButton = Instantiate(Button, MatchHolder);
                instButton.GetComponent<MatchButton>().MatchInfo = i;
                instButton.name = i.External.ToString();
                instButton.transform.GetChild(0).GetComponent<Text>().text
                    = i.External.ToString();
            }
        }
    }
}
