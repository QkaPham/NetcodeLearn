using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader InputReader;
    [SerializeField] private Transform turretTransform;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(InputReader.AimPosition);
        turretTransform.up = new Vector2(aimWorldPosition.x - turretTransform.position.x, aimWorldPosition.y - turretTransform.position.y);
    }
}
