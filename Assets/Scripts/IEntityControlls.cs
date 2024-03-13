
using UnityEngine;

public interface IEntityControlls
{
    public Animator GetAnimator();
    public GameObject GetGameObject();
    public Vector3 GetMovmentDirection();
    public void Movement(Vector3 movement);
    public void ManualMovement();
    void SwitchState(PlayerController.PlayerState controlling);
}
