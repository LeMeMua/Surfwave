using Unity.Cinemachine;
using UnityEngine;

public class StateBasePlayer : StateBase
{
    public GameObject playerObj;
    public GameObject orientation;

    public GameObject mira;

    public Player_Actions _inputActions;

    private Transform root;

    public override void Begin()
    {
        base.Begin();
        root = obj.transform.root;
        playerObj=root.Find("PersonajeCompleto").gameObject;
        orientation=root.Find("Orientation").gameObject;
        mira = mira;
    }
}
