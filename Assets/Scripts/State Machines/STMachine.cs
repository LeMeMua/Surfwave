using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class STMachine : MonoBehaviour
{
    private GameObject obj;
    [SerializeField] StateBase defaultState;

    [SerializeField] StateBase currentState;

    void Awake()
    {
        obj = transform.gameObject;
    }

    void Start()
    {
        currentState = defaultState;
        StartState();
    }
    void StartState()
    {
        currentState.obj = obj;
        currentState.stateMachine = this;
        currentState.Begin();
    }

    public void ChangeTo(string new_state)
    {
        if (currentState)
        {
            currentState.End();
            Transform child = transform.Find(new_state);
            currentState = child!= null ? child.GetComponent<StateBase>() : null;
            StartState();
        }
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
            currentState.OnFixedUpdate();
        }
    }

    void OnEnable()
    {
        if (currentState)
        {
            currentState.OnOnEnable();
        }
    }

    void OnDisable()
    {
        if (currentState)
        {
            currentState.OnOnDisable();
        }
    }

    void RecursiveUpdate(StateBase state)
    {
        if (state){
            StateBase parent = currentState.obj.transform.parent.GetComponent<StateBase>();
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
            StateBase parent = currentState.obj.transform.parent.GetComponent<StateBase>();
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
            StateBase parent = currentState.obj.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnOnEnable();
                RecursiveOnEnable(parent);
            }
        }      
    }
    void RecursiveOnDisable(StateBase state)
    {
        if (state){
            StateBase parent = currentState.obj.transform.parent.GetComponent<StateBase>();
            if (parent)
            {
                parent.OnOnDisable();
                RecursiveOnDisable(parent);
            }
        }      
    }
}
