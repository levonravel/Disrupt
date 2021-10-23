/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      N/A
 *
 *Class Information
 *      This enum determins if the packet is sequenced or reliable
 */
namespace RavelNet
{
    [System.Serializable]
    public enum Protocol
    {
        Reliable = 64,
        Sequenced,
    }
}
