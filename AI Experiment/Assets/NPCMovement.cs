using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public Transform target;
    public Transform facingTarget;
    public Animator animator;
    public float rotationSpeed = 5f;
    public float movementSpeed = 3f;
    public float stoppingDistance = 1.5f;
    private bool isWandering = false;
    private Vector3 randomTarget;

    private void Start()
    {
        SetRandomTarget();
    }

    private void Update()
    {
        if (facingTarget != null)
        {
            RotateTowardsTarget(facingTarget);
        }

        if (target != null)
        {
            // Rotate towards the target
            RotateTowardsTarget(target);

            // Move towards the target
            MoveTowardsTarget(target.position);
        }
        else if (!isWandering)
        {
            // Wander randomly
            WanderRandomly();
        }
        else
        {
            // Move towards the random target
            MoveTowardsTarget(randomTarget);

            // Check if reached the random target
            if (Vector3.Distance(transform.position, randomTarget) < stoppingDistance)
            {
                SetRandomTarget();
            }
        }
    }

    private void RotateTowardsTarget(Transform _target)
    {
        Vector3 targetDirection = _target.position - transform.position;
        targetDirection.y = 0f; // Ignore vertical component

        if (targetDirection.magnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void MoveTowardsTarget(Vector3 _target)
    {
        Vector3 targetDirection = _target - transform.position;

        if (targetDirection.magnitude > stoppingDistance)
        {
            // Move towards the target
            Vector3 movement = targetDirection.normalized * movementSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);

            // if the animator is not null, and there is a parameter called "speed", set it to 1
            if (animator != null && animator.parameters.Length > 0)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "speed")
                    {
                        animator.SetFloat("speed", 1f);
                    }
                }
            }
        }
        else
        {
            // if the animator is not null, and there is a parameter called "speed", set it to 0
            if (animator != null && animator.parameters.Length > 0)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "speed")
                    {
                        animator.SetFloat("speed", 0f);
                    }
                }
            }
        }
    }

    private void WanderRandomly()
    {
        isWandering = true;
        Invoke("SetRandomTarget", Random.Range(1f, 3f));
    }

    private void SetRandomTarget()
    {
        randomTarget = transform.position + new Vector3(
            Random.Range(-10f, 10f),
            0f,
            Random.Range(-10f, 10f)
        );

        isWandering = false;
    }
}
