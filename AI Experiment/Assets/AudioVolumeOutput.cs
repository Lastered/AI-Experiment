using UnityEngine;

public class AudioVolumeOutput : MonoBehaviour
{    
    public AudioSource audioSource;
    public float referenceLevel = 0.1f; // Reference level for calculating decibel level
    public float targetHeight = 1.0f; // Height above the original position to lerp to
    public float lerpSpeed = 5.0f; // Speed of the lerp movement

    private float originalYPosition;
    private float decibelOutput;
    private float targetYPosition;
    private bool isLerping;

    private void Start()
    {
        originalYPosition = transform.position.y;
        targetYPosition = originalYPosition;
        isLerping = false;
    }

    private void Update()
    {
        if (audioSource != null)
        {
            float[] samples = new float[512]; // Number of audio samples to retrieve
            audioSource.GetOutputData(samples, 0); // Get the audio samples

            float sum = 0.0f;
            foreach (float sample in samples)
            {
                sum += sample * sample; // Square each sample and accumulate
            }

            float rms = Mathf.Sqrt(sum / samples.Length); // Calculate root mean square
            decibelOutput = 20.0f * Mathf.Log10(rms / referenceLevel); // Calculate decibel level
        }
        else
        {
            decibelOutput = -Mathf.Infinity;
        }

        // Calculate the target position based on the decibel output
        targetYPosition = originalYPosition + (decibelOutput * targetHeight);

        // Start or stop lerping based on the delta of the decibel output
        if (Mathf.Abs(transform.position.y - targetYPosition) > 0.01f)
        {
            if (!isLerping)
            {
                isLerping = true;
            }
        }
        else
        {
            isLerping = false;
        }

        // Lerp towards the target position if lerping is enabled
        if (isLerping)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetYPosition, transform.position.z), lerpSpeed * Time.deltaTime);
        }
    }
}
