using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using TMPro;
using System.Collections.Generic;
using OpenAI;


[RequireComponent(typeof(AudioSource))]
public class VoiceGeneration : MonoBehaviour
{
    private ChatGPT chatGPT;


    [SerializeField] TextFileHandler authTextFileHandler;
    
    [SerializeField] bool useVoiceGeneration = true;

    public string defaultVoice = "11B3qed5NJ7d7AsFu6oH"; // Enter the voice you want to use here
    private string selectedVoice = "";


    [SerializeField] TextMeshProUGUI subtitleText;

    [SerializeField] float timeBetweenLines = 17f;

    [System.Serializable]
    public class Character
    {
        public string name;
        public string id;
        public NPCMovement target;
    }

    [SerializeField] Character[] characters;

    public AudioClip latestClip;

    private NPCMovement speakingCharacter = null;
    private NPCMovement targetCharacter;

    private int currentLine = 0;
    private int linePregnator = 0;

    public void GenerateMultipleVoices(string input)
    {
        StartCoroutine(LogDialogueWithDelay(input));
    }

    private IEnumerator LogDialogueWithDelay(string input)
    {
        string[] lines = input.Split('\n');

        foreach (string line in lines)
        {
            string[] parts = line.Split(':');

            if (parts.Length == 2)
            {
                string name = parts[0].Trim();
                string dialogue = parts[1].Trim();

                //if dialoge is empty, skip
                if (dialogue == "" || dialogue == " ")
                {
                    continue;
                }

                string characterID = GetCharacterID(name);

                if (characterID == "")
                {
                    characterID = defaultVoice;
                }

                
                //remove all brackets
                dialogue = dialogue.Replace("(", "");
                dialogue = dialogue.Replace(")", "");
                

                if (dialogue.StartsWith("to "))
                {
                    string[] parts2 = dialogue.Split(' ');
                    string targetName = parts2[1].Trim();
                    targetCharacter = GetCharacter(targetName);
                    if (targetCharacter != null)
                    {
                        speakingCharacter = GetCharacter(name);
                    }

                    dialogue = dialogue.Substring(dialogue.IndexOf(" ") + 1);
                    dialogue = dialogue.Substring(dialogue.IndexOf(" ") + 1);
                }
                else
                {
                    targetCharacter = null;
                    speakingCharacter = null;
                }



                //remove all quotes
                dialogue = dialogue.Replace("\"", "");
                dialogue = dialogue.Replace("'", "");

                // remove all

                if (useVoiceGeneration)
                {
                    characterID = characterID.Replace(" ", "");
                    dialogue = dialogue.Replace("\"", "");

                    Debug.Log("Sending request: " + dialogue);

                    StartCoroutine(SendSpeechRequest(dialogue, characterID, speakingCharacter, targetCharacter, linePregnator, name));
                    linePregnator++;
                    speakingCharacter = null;
                    targetCharacter = null;

                }




            }
        }

        yield return null;
    }

    private string GetCharacterID(string characterName)
    {
        foreach (Character character in characters)
        {
            if (character.name.ToLower() == characterName.ToLower())
            {
                return character.id;
            }
        }

        Debug.Log("Character not found: " + characterName);
        return "";
    }

    private NPCMovement GetCharacter(string characterName)
    {
        foreach (Character character in characters)
        {
            if (character.name.ToLower() == characterName.ToLower())
            {
                return character.target;
            }
        }

        return null;
    }

    void Start()
    {
        selectedVoice = defaultVoice;
        chatGPT = GetComponent<ChatGPT>();
        
    }

    public IEnumerator SendSpeechRequest(string text = "Your text here", string voicemodel_uuid = "0d830118-8e09-4d6a-98a2-abdc9a7e0a68", NPCMovement _character = null, NPCMovement _target = null, int _line = 0, string _name = "", bool _finalLine = false)
    {

        string url = "https://api.uberduck.ai/speak-synchronous";

        // for each line in the auth file, add it to the auth key

        string authKey = "Basic cHViX2Z3ZnFub2J3a3puamJlaXpsajpwa181MGQ3MzRhNS0zMDg5LTRhOWUtOGFjMi03MGFjZGYwMjEzMDc=";
    
        //remove quotes from auth key
        authKey = authKey.Replace("\'", "");

        

        string jsonPayload = "{\"speech\":\"" + text + "\",\"voicemodel_uuid\":\"" + voicemodel_uuid + "\"}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            using (var uploadHandler = new UploadHandlerRaw(jsonBytes))
            {
                request.uploadHandler = uploadHandler;
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                request.SetRequestHeader("Authorization", authKey);

                // wait until the current line is 3 off from the line we're trying to play
                yield return new WaitUntil(() => currentLine >= _line - 3);


                while (true)
                {

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        byte[] audioData = request.downloadHandler.data;
                        if (audioData == null || audioData.Length == 0)
                        {
                            Debug.Log("Failed to get audio data from the request");
                            yield break;
                        }

                        float[] audioDataFloat = new float[audioData.Length / 2];
                        for (int j = 0; j < audioData.Length / 2; j++)
                        {
                            short value = (short)((audioData[j * 2 + 1] << 8) | audioData[j * 2]);
                            audioDataFloat[j] = value / 32768f;
                        }

                        AudioClip audioClip = AudioClip.Create("AudioClip", audioDataFloat.Length, 1, 22050, false);
                        audioClip.SetData(audioDataFloat, 0);

                        // wait until the current line is the same as the line we're trying to play
                        yield return new WaitUntil(() => currentLine == _line);

                        // wait until the previous clip is done playing
                        yield return new WaitWhile(() => audioSource.isPlaying);

                        // wait a bit before playing the next clip
                        yield return new WaitForSeconds(timeBetweenLines);

                        // if it is the third to last line, call on chatGPT to generate the next script
                        if (currentLine == _line - 3)
                        {
                            chatGPT.SendReply();
                        }

                        audioSource.clip = audioClip;
                        latestClip = audioClip;
                        audioSource.Play();
                        
                        currentLine++;



                        Camera.main.GetComponent<CameraDirector>().CameraDice();

                        //remove all targets from all NPCmovement scripts, and make them face the speaker if they are not the speaker
                        var allNPCs = FindObjectsOfType<NPCMovement>();
                        foreach (NPCMovement npc in allNPCs)
                        {
                            

                            npc.target = null;                            
                            
                            if (npc != _character && npc.facingTarget == null && _character != null)
                            {
                                npc.facingTarget = _character.transform;
                            }
                        }

                        if (_character != null && _target != null)
                        {
                            _character.target = _target.transform;
                            _target.target = _character.transform;
                        }

                        if (subtitleText != null)
                        {
                            subtitleText.text = _name + ": " + text;
                        }

                        yield break; // Exit the function if the request was successful
                    }
                    else
                    {
                        // wait for the audio to finish playing before continuing


                        if (request.responseCode == 403)
                        {
                            Debug.Log("Forbidden error, retrying with the next key");

                            // Only delete the line if the authentication key doesn't work
                            authTextFileHandler.DeleteUppermostLine();



                            authKey = authTextFileHandler.GetUppermostLine();

                            // only continue if there is at least one more auth key
                            if (authKey != "")
                                continue; // Try the next auth key
                        }

                        if (request.responseCode == 429)
                        {
                            Debug.Log("Too many requests, retrying in 5 seconds");
                            yield return new WaitForSeconds(5);
                            continue;
                        }

                        if (request.responseCode == 500)
                        {
                            Debug.Log("Internal server error, retrying in 5 seconds");
                            yield return new WaitForSeconds(5);
                            continue;
                        }

                        if (request.responseCode == 503)
                        {
                            Debug.Log("Service unavailable, retrying in 5 seconds");
                            yield return new WaitForSeconds(5);
                            continue;
                        }

                        if (request.responseCode == 504)
                        {
                            Debug.Log("Gateway timeout, retrying in 5 seconds");
                            yield return new WaitForSeconds(5);
                            continue;
                        }

                        if (request.responseCode == 400)
                        {
                            Debug.Log("Bad request, skipping line");
                            currentLine++;
                            subtitleText.text = "(Incomprehensible noise)";
                            // if it is the third to last line, call on chatGPT to generate the next script
                            if (currentLine == _line - 3)
                            {
                                chatGPT.GenerateNewScript();
                            }
                            yield break;


                        }

                        

                        Debug.Log("Request failed with error: " + request.error + " \nand response code: " + request.responseCode + "\n" + text);
                        break; // Exit the loop if there is an error other than forbidden or no more auth keys
                    }
                }

                if (authTextFileHandler.GetNumberOfLines() == 0)
                    Debug.Log("All auth keys failed, unable to make a successful request");
            }
        }
    }




    private const string AuthorizationToken = "cHViX2pmY2hxeWZwbmRnZmxhYXFncTpwa19lZDk0ODcyNC1mYjViLTQ4MDQtOWRkYi1lNjdkOTk5YmI3NDE=";
    public AudioSource audioSource;
}

