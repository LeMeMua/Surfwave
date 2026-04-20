using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEngine;

public class StateBase : MonoBehaviour
{
    [SerializeField] GameObject _obj; 
    public GameObject obj { get=> _obj; set => _obj = value;}
    private STMachine _stateMachine;

    public STMachine stateMachine { get=> _stateMachine; set => _stateMachine = value;}

    void Awake()
    {
        obj = transform.parent.gameObject;
    }
    public void Begin()
    {
        
    }

    public void End()
    {
        
    }

    public void OnUpdate()
    {
        
    }

    public void OnFixedUpdate()
    {
        
    }

    public void OnOnEnable()
    {
        
    }

    public void OnOnDisable()
    {
        
    }
}
