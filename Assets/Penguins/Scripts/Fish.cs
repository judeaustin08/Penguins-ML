using UnityEngine;

public class Fish : MonoBehaviour
{
    [Tooltip("The swim speed")]
    public float fishSpeed;

    private float randomizedSpeed = 0;
    private float nextActionTime = -1;
    private Vector3 targetPosition;

    private void FixedUpdate()
    {
        if (fishSpeed > 0)
        {
            Swim();
        }
    }

    /// <summary>
    /// Swim between random positions
    /// </summary>
    private void Swim()
    {
        // If it's time for the next action, pick a new destination and speed
        // Else, swim toward the destination
        if (Time.fixedTime >= nextActionTime)
        {
            // Randomize the speed
            randomizedSpeed = fishSpeed * Random.Range(0.5f, 1.5f);

            // Pick a random target
            targetPosition = PenguinArea.ChooseRandomPosition(transform.parent.position, 100, 260, 2, 13);

            // Rotate toward the target
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

            // Calculate the time to get there
            float eta = Vector3.Distance(targetPosition, transform.position) / randomizedSpeed;
            nextActionTime = Time.fixedTime + eta;
        }
        else
        {
            Vector3 moveVector = randomizedSpeed * transform.forward * Time.fixedDeltaTime;

            // Make sure the fish does not move past the target position
            if (moveVector.magnitude <= Vector3.Distance(targetPosition, transform.position))
            {
                transform.position += moveVector;
            }
            else
            {
                transform.position = targetPosition;
                nextActionTime = -1;
            }
        }
    }
}
