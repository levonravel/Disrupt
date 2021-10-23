/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      N/A
 *
 *Class Information
 *      This enum helps determine the packet type sent over the network
 */
namespace RavelNet
{
    public enum Flags
    {
        None,
        NatReq,
        NatIntro,
        UPD,
        Dat,
        Dc,
        Con,
        NatHost,
        HostList,
    }
}
