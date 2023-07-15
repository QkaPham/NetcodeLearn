using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet wallet;
    [Header("Configs")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;
    private float timer;
    private float muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }


    private void Update()
    {
        if (muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0)
            {
                muzzleFlash.SetActive(false);
            }

        }

        if (!IsOwner) return;
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (!shouldFire) return;
        if (timer > 0) return;
        if (wallet.TotalCoins.Value < costToFire) return;

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.transform.rotation);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.transform.rotation);
        timer = 1 / fireRate;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Quaternion rotation)
    {
        if (wallet.TotalCoins.Value < costToFire) return;
        wallet.SpendCoins(costToFire);
        var projectile = Instantiate(serverProjectilePrefab, spawnPos, rotation);
        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

        if (projectile.TryGetComponent<DealDamageOnContact>(out var dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
        SpawnDummyProjectileClientRpc(spawnPos, rotation);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Quaternion rotation)
    {
        if (IsOwner) return;
        SpawnDummyProjectile(spawnPos, rotation);
    }

    private void HandlePrimaryFire(bool fireInput)
    {
        shouldFire = fireInput;
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Quaternion rotation)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        var projectile = Instantiate(clientProjectilePrefab, spawnPos, rotation);
        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());
        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }

}
