using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowPlayer : MonoBehaviour
{
    public Transform player;
    NavMeshAgent agent;

    public float escapeTime = 5f;
    public float blinkSpeed = 0.15f;

    Collider enemyCollider;
    Renderer[] renderers;

    bool isDisabled;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        transform.rotation = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);

        // Evita que el NavMeshAgent incline o acueste al enemigo
    }

    void Update()
    {
        if (player != null && !isDisabled)
        {
            agent.SetDestination(player.position);
        }

        RotateOnlyOnY();
    }

    void RotateOnlyOnY()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            // Esto hace que el RIGHT del enemigo apunte hacia el jugador
            targetRotation *= Quaternion.Euler(0f, -90f, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (isDisabled) return;

        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.stars = Mathf.Max(0, GameManager.instance.stars - 1);

            Debug.Log("El enemigo tocó al jugador. Estrellas: " + GameManager.instance.stars);

            StartCoroutine(DisableEnemyCollision());
        }
    }

    IEnumerator DisableEnemyCollision()
    {
        isDisabled = true;

        enemyCollider.enabled = false;

        if (agent != null)
        {
            agent.isStopped = true;
        }

        float timer = 0f;

        while (timer < escapeTime)
        {
            foreach (Renderer rend in renderers)
            {
                rend.enabled = !rend.enabled;
            }

            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed;
        }

        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        enemyCollider.enabled = true;

        if (agent != null)
        {
            agent.isStopped = false;
        }

        isDisabled = false;
    }
}