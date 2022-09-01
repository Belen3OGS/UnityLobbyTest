using Unity.Networking.Transport;
using UnityEngine;

public class UTPClient : MonoBehaviour
{
    protected NetworkDriver driver;
    protected NetworkConnection connection;

    private void Start()
    {
        driver = NetworkDriver.Create();
        //NetworkEndPoint endpoint = NetworkEndPoint.TryParse();
    }

    private void Update()
    {
        
    }


    private void OnDestroy()
    {
        
    }
}