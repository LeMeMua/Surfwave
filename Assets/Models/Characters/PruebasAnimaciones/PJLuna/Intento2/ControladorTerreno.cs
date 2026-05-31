using UnityEngine;

public class ControladorTerreno : MonoBehaviour
{
    private Animator animator;
    public GameObject tablaDeSurf; // arrastra la tabla aquí en Inspector

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        DetectarTerreno();
    }

    void DetectarTerreno()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            bool enSurf = hit.collider.CompareTag("Surf");
            animator.SetBool("EnSurf", enSurf);
            tablaDeSurf.SetActive(enSurf);
        }
    }
}