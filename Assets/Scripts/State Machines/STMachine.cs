using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class STMachine : MonoBehaviour
{
    GameObject obj;
    [SerializeField] StateBase defaultState;

    [SerializeField] StateBase currentState;

    public GameObject cam;

    public GameObject aimCam;

    void Awake()
    {
        
        obj = transform.gameObject;
    }

    void Start()
    {
        currentState = defaultState;
        StartStateRecursive(currentState);
        RecursiveAwake(currentState);
        RecursiveOnEnable(currentState);
    }
    void StartStateRecursive(StateBase state)
    {
        StateBase parent = state.transform.parent.GetComponent<StateBase>();
        if (parent)
        {
            StartStateRecursive(parent);
            
        }
        state.obj=obj;
        state.stateMachine=this;
        state.Begin();
        
    }

    void EndStateRecurisve(StateBase state)
    {
        StateBase parent = state.transform.parent.GetComponent<StateBase>();
        if (parent)
        {
            EndStateRecurisve(parent);
            
        }
        state.End();
        state.OnOnDisable();
    }

    public void ChangeTo(string new_state)
    {
        if (currentState != null)
        {
            currentState.End();
        }

        Transform target = FindRecursive(transform, new_state);

        currentState = target.GetComponent<StateBase>();

        StartStateRecursive(currentState);
        RecursiveOnEnable(currentState);
    }

    void Update()
    {
        if (currentState)
        {
            RecursiveUpdate(currentState);
        }
    }

    void FixedUpdate()
    {
        if (currentState)
        {
            RecursiveFixedUpdate(currentState);
        }
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        if (currentState)
        {
            RecursiveOnDisable(currentState);
        }
    }

    void OnDestroy()
    {
        if (currentState)
        {
            RecursiveOnDestroy(currentState);
        }
    }

    void RecursiveUpdate(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnUpdate();
                RecursiveUpdate(parent);
            }
            state.OnUpdate();
        }      
    }

    void RecursiveFixedUpdate(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnFixedUpdate();
                RecursiveFixedUpdate(parent);
            }
            state.OnFixedUpdate();
        }      
    }
    void RecursiveOnEnable(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                RecursiveOnEnable(parent);
            }
            state.OnOnEnable();
        }      
    }
    void RecursiveOnDisable(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnOnDisable();
                RecursiveOnDisable(parent);
            }
        }      
    }
    void RecursiveOnDestroy(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnOnDestroy();
                RecursiveOnDisable(parent);
            }
        }      
    }
    void RecursiveAwake(StateBase state)
    {
        if (state){
            StateBase parent = state.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                RecursiveAwake(parent);
            }
            state.OnAwake();
        }      
    }

    Transform FindRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
