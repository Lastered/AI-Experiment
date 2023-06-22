using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

[RequireComponent(typeof(AudioSource))]
public class VoiceSynthesisElevenLabs : MonoBehaviour
{

    [SerializeField] string startText;


    
    [SerializeField] bool useVoiceGeneration = true;

    public string elevenLabsapiKey = ""; // Enter your ElevenLabs API key here
    public string elevenLabsVoice = "11B3qed5NJ7d7AsFu6oH"; // Enter the voice you want to use here
    private string selectedElevenLabsVoice = "";

    private const string elevenLabsApiUrl = "https://api.elevenlabs.io/v1/text-to-speech/";

    [SerializeField] float timePerCharacter = 17f;



    void Start()
    {
        // Generate the voice
        if (startText != "")
            GenerateVoice(startText);
    }


    public void GenerateMultipleVoices(string input)
    {
        StartCoroutine(LogDialogueWithDelay(input));
    }

    private IEnumerator LogDialogueWithDelay(string input)
    {
        string[] lines = input.Split('\n');

        foreach (string line in lines)
        {
            // Split the line into name and dialogue
            string[] parts = line.Split(':');

            if (parts.Length == 2)
            {
                string name = parts[0].Trim();
                string dialogue = parts[1].Trim();




                // Log the character name, ID, and dialogue
                Debug.Log(name + ", " + dialogue);

                if (useVoiceGeneration)
                {
                    // Generate the voice
                    GenerateVoice(dialogue);

                }

                //role the dice to see if the camera will change
                Camera.main.GetComponent<CameraDirector>().CameraDice();

                // calculate the time it takes to say the line
                float time = dialogue.Length / timePerCharacter;

                // Wait for the specified time
                yield return new WaitForSeconds(time);
            }
        }
    }

    public void GenerateVoice(string text, string voiceId = null)
    {
        if (voiceId == null)
            voiceId = elevenLabsVoice;

        selectedElevenLabsVoice = voiceId;

        StartCoroutine(GenerateVoiceCoroutine(text));
    }

    private IEnumerator GenerateVoiceCoroutine(string text)
    {
        Debug.Log("Generating voice for: " + text);





        // Prepare the request payload
        var payload = new { text = text, model_id = "eleven_monolingual_v1" };
        string jsonPayload = JsonConvert.SerializeObject(payload);
        byte[] requestData = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        // Create the request
        using (UnityWebRequest request = new UnityWebRequest(elevenLabsApiUrl + selectedElevenLabsVoice, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestData);
            request.downloadHandler = new DownloadHandlerAudioClip("", AudioType.MPEG);
            request.SetRequestHeader("accept", "audio/mpeg");
            request.SetRequestHeader("xi-api-key", elevenLabsapiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request
            yield return request.SendWebRequest();

            // Check for errors
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Voice generation request failed: " + request.error);
            }
            else
            {
                // Get the generated audio clip
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

                // Play the audio clip
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
    }
}
