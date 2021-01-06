/*
//.................................Body Must Remain Intact For Usage..........................................//
//																											  //
//  I would appreciate you not pirating this, it helps motivate me to contribute back with more updates		  //
//     also this asset doesnt have copy protection in the hopes of making lives of developers easier          //
//                                    so please do the right thing                                            //
//                                                                                                            //
//     if you find something missing and add onto this script please feel free to let me know via             //
//                              email I would love to update with changes                                     //
//                           also contributing helps other fellow developers                                  //
//		                                                                                                      //
//		                                     Best Regards                                                     //
//		                                     Levon M Ravel                                                    // 
//																											  //
//............................................................................................................//

    File:		Upnp.cs

    Contains	Nat Traversal Upnp
    Version	    1.1


    Dependencies  	Unity5 v 5.1 Free
                    CrossNet.cs					

    File Ownership  Levon Marcus Ravel
    Developer       Levon Marcus Ravel
    Contact         LevonRavel@numbapple.com
    Copyright		2015 by NumbApple Interactive, all rights reserved



    Change History (most recent first):

    Initial Version
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System;

namespace RavelTek.Disrupt.Network_Utilities
{
    static public class Upnp
    {
        static string xmlURL = "";
        static string RouterIP = "";
        static string ControlType ="";
        static string ServiceType = "";
        static string insertIP ="";
        static string port = "";
        static string productName = ""; 
        static bool Gateway;
        static bool webPort;
        static List<string> map_layout;

        public static void Discover(string Ip,string Port,string ProductName)
        {
            insertIP = Ip;
            port = Port;
            productName = ProductName;

            if(productName.Contains ("WebClient v1.0"))
               webPort = true;
    
            Thread thd = new Thread(Response);
            thd.Start();
        }
        static void Response()
        {
            bool finished = false;

            Stopwatch sw = new Stopwatch ();
            sw.Start ();
            string[] foundDevices = new string[200];
            int deviceCount = 0;
            
            var ssdpMsg = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1900\r\nMAN:\"ssdp:discover\"\r\nST:ssdp:all\r\nMX:5\r\n\r\n";
            var ssdpPacket = Encoding.ASCII.GetBytes(ssdpMsg);
            IPAddress multicastAddress = IPAddress.Parse ("239.255.255.250");
            var socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddress, IPAddress.Any));
            socket.SendTo(ssdpPacket,SocketFlags.None, new IPEndPoint(multicastAddress, 1900));
            var response = new byte[8000];
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            socket.Blocking = false;

            while (!finished)
            {	
                try
                {					
                    socket.ReceiveFrom(response, ref ep);
                    var str = Encoding.UTF8.GetString(response);
                    var aSTR = str.Split ('\n');
                    foreach (string i in aSTR)
                    {
                        
                        if(i.IndexOf (("SERVER: "),StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            string newDev = i.Replace ("Server: ","");
                            if(!foundDevices.Contains (newDev))
                            {
                                foundDevices[deviceCount] = newDev;
                                deviceCount++;
                            }
                        }
                        
                        if(i.IndexOf (("LOCATION: "),StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            xmlURL = Regex.Replace(i, "Location: ", "", RegexOptions.IgnoreCase);
                            RouterIP = GetBetween (i,"http://","/");
                        }
                        
                        if(i.IndexOf (("InteDisruptwork.NetworkingGatewayDevice"),StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            Gateway = true;
                        }					
                        
                        
                        if(Gateway)
                        {
                            if(i.Contains ("WANPPPConnection") && ServiceType == "")
                            {
                                ServiceType = i.Replace("ST: ","");
                                ServiceType = ServiceType.Replace ("\r","");
                            }
                            
                            if(i.Contains ("WANIPConnection") && ServiceType == "")
                            {
                                ServiceType = i.Replace("ST: ","");
                                ServiceType = ServiceType.Replace ("\r","");
                            }
                        }
                    }
                    
                    if(ServiceType != "" && RouterIP != "" && Gateway)
                    {
                        Gateway = false;
                        WebClient webClient = new WebClient();
                        
                        try
                        {		
                            string routerXML = webClient.DownloadString(xmlURL);
                            int strIndex = routerXML.IndexOf (ServiceType);
                            ControlType = GetBetween(routerXML.Substring (strIndex),"<controlURL>","</controlURL>");

                        }catch(System.Exception ex){Debugging(ex.Message);}
                        
                        finally{webClient.Dispose();}

                        socket.Close ();
                        sw.Stop();
                        GetMappings();
                        finished = true;
                    }
                }catch{}

                if(sw.Elapsed.Seconds > 4)
                {
                    socket.Close ();
                    finished = true;
                }
            }
        }
        static void GetMappings()
        {
            map_layout = new List<string> ();
            string xml_response = "";
            int i = 0;
            int dex = 0;
            switch (i)
            {	

            case 0:
                var soapBody =
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body><u:"
                    + "GetGenericPortMappingEntry"
                    + "xmlns:u=\"" + ServiceType + "\">"
                    + "<NewPortMappingIndex>"+dex.ToString ()+"</NewPortMappingIndex>"
                    + "<NewRemoteHost></NewRemoteHost>"
                    + "<NewExternalPort></NewExternalPort>"
                    + "<NewProtocol></NewProtocol>"
                    + "<NewInternalPort></NewInternalPort>"
                    + "<NewInternalClient></NewInternalClient>"
                    + "<NewEnabled>1</NewEnabled>"
                    + "<NewPortMappingDescription></NewPortMappingDescription>"
                    + "<NewLeaseDuration></NewLeaseDuration>"
                    + "</u:GetGenericPortMappingEntry></s:Body></s:Body></s:Envelope>\r\n\r\n";
                
                byte[] body = System.Text.UTF8Encoding.ASCII.GetBytes (soapBody);
                var url = "http://" + RouterIP + ControlType;

                try 
                {
                    var wr = HttpWebRequest.Create (url);
                    wr.Method = "POST";
                    wr.Headers.Add ("SOAPAction", "\"" + ServiceType + "#GetGenericPortMappingEntry" + "\"");
                    wr.ContentType = "text/xml;charset=\"utf-8\"";
                    wr.ContentLength = body.Length;			
                    var stream = wr.GetRequestStream ();
                    stream.Write (body, 0, body.Length);			
                    stream.Flush ();
                    stream.Close ();
                    
                    using (HttpWebResponse response = (HttpWebResponse)wr.GetResponse()) {
                        Stream receiveStream = response.GetResponseStream ();			
                        StreamReader readStream = new StreamReader (receiveStream, Encoding.UTF8);			
                        xml_response = readStream.ReadToEnd ();
                        response.Close ();
                        readStream.Close ();
                    }
                } catch{
                    goto case 2;
                }			
                goto case 1;
                
            case 1:
                string thisip = GetBetween (xml_response, "<NewInternalClient>", "</NewInternalClient>");
                string thisport = GetBetween (xml_response,"<NewInternalPort>", "</NewInternalPort>");
                string thisprod = GetBetween (xml_response,"<NewPortMappingDescription>","</NewPortMappingDescription>");

                if(thisip == insertIP && thisprod == productName)
                {

                    if(webPort)
                    {
                        Debugging ("WebPort Already Mapped "+ thisport);

                    }else{

                        Debugging ("Port Already Mapped "+ thisport);
                    }

                    break;
                }
                dex++;
                map_layout.Add (thisport);
                goto case 0;

            case 2:
                for(int p = int.Parse (port); p < float.MaxValue;p++)
                {
                    if(!map_layout.Contains (p.ToString ()))
                    {
                        port = p.ToString ();
                        SetPort ();
                        break;
                    }
                }
                break;
            }						
        }		
        static void SetPort()
        {
            string portType = "";

            if (!webPort)
                portType = "UDP";
            else
                portType = "TCP";

            var soapBody =
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body><u:"
                    + "AddPortMapping"
                    + " xmlns:u=\"" + ServiceType + "\">"
                    + "<NewRemoteHost></NewRemoteHost>"
                    + "<NewExternalPort>" + port + "</NewExternalPort>"
                    + "<NewProtocol>"+portType+"</NewProtocol>"
                    + "<NewInternalPort>" + port + "</NewInternalPort>"
                    + "<NewInternalClient>" + insertIP + "</NewInternalClient>"
                    + "<NewEnabled>1</NewEnabled>"
                    + "<NewPortMappingDescription>" + productName + "</NewPortMappingDescription>"
                    + "<NewLeaseDuration>0</NewLeaseDuration>"
                    + "</u:AddPortMapping></s:Body></s:Body></s:Envelope>\r\n\r\n";	
            
            byte[] body = System.Text.UTF8Encoding.ASCII.GetBytes(soapBody);
            var url = "http://" + RouterIP+ControlType;
            var wr = HttpWebRequest.Create(url);
            wr.Method = "POST";
            wr.Headers.Add("SOAPAction","\""+ServiceType+"#"+"AddPortMapping"+"\"");
            wr.ContentType = "text/xml;charset=\"utf-8\"";
            wr.ContentLength = body.Length;			
            var stream = wr.GetRequestStream();
            stream.Write(body, 0, body.Length);			
            stream.Flush();
            stream.Close();

            Debugging ("Using Port: " + port);
        }
        static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }
        static void Debugging(string msg)
        {
            String THISMSG = msg;
        }
    }
}