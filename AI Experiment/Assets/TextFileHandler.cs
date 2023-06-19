using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextFileHandler : MonoBehaviour
{
    public string filePath; // Path to the .txt file
    public TextMeshProUGUI subtitleText; // TextMeshProUGUI to display the text
    public bool extractAuthKeys = false; // Set to true to extract auth keys

    private void Start()
    {
        string uppermostLine = GetUppermostLine();

        if (subtitleText != null)
            RandomLoading();

        if (extractAuthKeys)
            ExtractAuthKeysFromText();
        



    }

    void ExtractAuthKeysFromText()
    {
        string[] lines = File.ReadAllLines(filePath);
        List<string> authKeyList = new List<string>();

        string allText = string.Join("\n", lines);


        // split the words between "'Basic"
        string[] words = allText.Split(new string[] { "'Basic" }, System.StringSplitOptions.None);



        foreach (string word in words)
        {
            // split the word at "='"
            string[] b = word.Split(new string[] { "='" }, System.StringSplitOptions.None);

            // add the first part of the word to the list
            authKeyList.Add("'Basic" + b[0] + "='");

        }
        

        string[] authKeys = authKeyList.ToArray();

        //remove duplicates
        authKeys = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Distinct(authKeys));

        Debug.Log("Number of auth keys: " + authKeys.Length);

        // replace the file with the auth keys
        File.WriteAllLines(filePath, authKeys);
    }





    void RandomLoading()
    {
        // only proceed if subtitleText is not null
        if (subtitleText != null)
        {
            // set the subtitles text to a random line from the text file
            subtitleText.text = GetRandomLine();

            Invoke("RandomLoading", 5f);
            
        }



    }

    public string GetUppermostLine()
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length > 0)
            return lines[0];

        return string.Empty; // Return an empty string if the file is empty
    }

    public string GetLine(int lineNumber)
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length > 0)
            return lines[lineNumber];

        return string.Empty; // Return an empty string if the file is empty
    }

    public string GetRandomLine()
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length > 0)
            return lines[Random.Range(0, lines.Length)];

        return string.Empty; // Return an empty string if the file is empty
    }

    public void DeleteUppermostLine()
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length > 0)
        {
            string[] newLines = new string[lines.Length - 1];
            System.Array.Copy(lines, 1, newLines, 0, newLines.Length);
            File.WriteAllLines(filePath, newLines);
        }
    }

    public int GetNumberOfLines()
    {
        string[] lines = File.ReadAllLines(filePath);
        return lines.Length;
    }

    // get all lines
    public string[] GetAllLines()
    {
        string[] lines = File.ReadAllLines(filePath);
        return lines;
    }
    
}
