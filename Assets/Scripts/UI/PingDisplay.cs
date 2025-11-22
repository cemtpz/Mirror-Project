using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private float updateInterval = 0.5f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;

            if (NetworkClient.isConnected)
            {
                // RTT saniye cinsinden -> milisaniyeye çevirelim
                int ping = Mathf.RoundToInt((float)(NetworkTime.rtt * 1000f));
                pingText.text = $"Ping: {ping} ms";
            }
            else
            {
                pingText.text = "Ping: ---";
            }
        }
    }
}
