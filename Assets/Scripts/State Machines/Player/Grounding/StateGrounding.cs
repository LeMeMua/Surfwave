using UnityEngine;

public class StateGrounding : StateBasePlayer
{
    public Rigidbody body;

    public bool isGrounded;
    public override void Begin()
    {
        base.Begin();
        body=playerObj.GetComponent<Rigidbody>();
        body.linearDamping = 3;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void End()
    {
        base.End();
        body.constraints = RigidbodyConstraints.None;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        CheckGround();
    }

    void CheckGround()
    {
        isGrounded= Physics.Raycast(playerObj.transform.position, Vector3.down, 2f, WavesManager.instance.mask);
    }
}
