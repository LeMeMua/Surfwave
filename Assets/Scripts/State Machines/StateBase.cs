
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
        //obj = transform.parent.gameObject;
    }
    public virtual void Begin()
    {
        
    }

    public virtual void End()
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual void OnFixedUpdate()
    {
        
    }

    public virtual void OnOnEnable()
    {
        
    }

    public virtual void OnOnDisable()
    {
        
    }

    public virtual void OnOnDestroy()
    {
        
    }

    public virtual void OnAwake()
    {
        
    }
}
