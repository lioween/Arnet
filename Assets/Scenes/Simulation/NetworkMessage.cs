using UnityEngine;

public class NetworkMessage
{
    public string SenderIP;
    public string DestinationIP;
    public string Content;

    public NetworkMessage(string sender, string destination, string content)
    {
        SenderIP = sender;
        DestinationIP = destination;
        Content = content;
    }
}
