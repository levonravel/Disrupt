using System;
using System.Collections.Generic;
using System.Net;

namespace RavelTek.Disrupt
{
    public class HostManagement
    {
        private readonly Dictionary<string, List<NatInfo>> hosts = new Dictionary<string, List<NatInfo>>();
        private static readonly object hostlist = new object();
        public int ConnectionIdCount;
        /// <summary>
        /// Do not use this call  **this is for the relay server only**
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public List<NatInfo> ServerOnlyHostList(string appid)
        {
            lock (hostlist)
            {
                if (!hosts.TryGetValue(appid, out List<NatInfo> matches))
                    return null;

                return hosts[appid];
            }
        }
        /// <summary>
        /// Do not use this call  **this is for the relay server only**
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public bool ServerOnlyAddHost(NatInfo natInfo, string appid)
        {
            lock (hostlist)
            {
                if (hosts.TryGetValue(appid, out List<NatInfo> matches))
                {
                    foreach (var match in matches)
                    {
                        if (match.External.Equals(natInfo.External))
                        {
                            return false;
                        }
                    }
                    Console.WriteLine("host request {0}", natInfo.External);
                    matches.Add(natInfo);
                    return true;
                }
                else
                {
                    Console.WriteLine("host request {0}", natInfo.External);
                    hosts.Add(appid, new List<NatInfo>() { natInfo });
                    return true;
                }
            }
        }
        /// <summary>
        /// Do not use this call  **this is for the relay server only**
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public bool ServerOnlyRemoveHost(EndPoint endpoint)
        {
            lock (hostlist)
            {
                var appId = "";
                NatInfo natInfo = null;
                List<NatInfo> found = new List<NatInfo>();

                foreach (var kvp in hosts)
                {
                    foreach (var host in kvp.Value)
                    {
                        if (host.External.Equals(endpoint))
                        {
                            appId = kvp.Key;
                            natInfo = host;

                            found.Add(natInfo);
                        }
                    }
                }

                if (found.Count == 0) return false;

                foreach (var i in found)
                {
                    hosts[appId].Remove(i);
                }

                return true;
            }
        }
    }
}
