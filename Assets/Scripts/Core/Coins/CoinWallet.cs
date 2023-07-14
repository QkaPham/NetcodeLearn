﻿using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out var coin)) return;
        int coinValue = coin.Collect();

        if (!IsServer) return;
        TotalCoins.Value += coinValue;
    }
}