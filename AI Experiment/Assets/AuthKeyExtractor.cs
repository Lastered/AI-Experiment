using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AuthKeyExtractor : MonoBehaviour
{
    public bool extractAuthKeys = false; // Set to true to extract auth keys
    public string inputString; // Input string containing auth keys

#if UNITY_EDITOR
    private void Update()
    {
        if (!EditorApplication.isPlaying && !EditorApplication.isPaused && extractAuthKeys)
        {
            extractAuthKeys = false;

            //clear console
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);


            ExtractAuthKeyFromLine(inputString);
        }
    }
#endif

    [ContextMenu("Extract Auth Keys")]
    public void ExtractAuthKeys()
    {
        string[] lines = inputString.Split('\n'); // Split the input string into lines

        // Iterate through each line and extract auth keys
        foreach (string line in lines)
        {
            string authKey = ExtractAuthKeyFromLine(line);
            if (!string.IsNullOrEmpty(authKey))
            {
                // Print the auth key to the console
                Debug.Log(authKey);
            }
        }
    }

    private string ExtractAuthKeyFromLine(string line)
    {

        string[] words = inputString.Split(" "); // Split the input string into words

        // Iterate through each word and check for auth keys
        foreach (string word in words)
        {
            if (word.Contains("sdk.auth('Basic"))
            {
                int startIndex = line.IndexOf("Basic") + 6; // Find the starting index of the auth key
                int endIndex = line.IndexOf("'", startIndex); // Find the ending index of the auth key
                Debug.Log("Basic " + line.Substring(startIndex, endIndex - startIndex)); // Extract the auth key

                // copy to clipboard
                TextEditor te = new TextEditor();
                te.text = "Basic " + line.Substring(startIndex, endIndex - startIndex);
                te.SelectAll();
                te.Copy();
                
            }
        }


        return null;
    }

    
}
