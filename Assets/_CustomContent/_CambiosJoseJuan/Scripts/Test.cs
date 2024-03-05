using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    int c = 0;
    [ContextMenu("Execute")]
    public void Execute()
    {
        GameObject.FindObjectOfType<CustomNetworkRoomManager>().StartClient();
        // NetworkClient.Connect("127.0.0.1");
    }

    public void Update()
    {
        // Debug.Log(NetworkClient.isConnected);
    }
}
