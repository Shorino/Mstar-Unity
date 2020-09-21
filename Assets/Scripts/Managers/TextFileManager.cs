using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public struct FilePath
{
    public string name, path;
};

public class TextFileManager : MonoBehaviour
{
    public static TextFileManager singleton;

    public static string folderPath = "c://Mstar Unity/Beatmaps/";
    public static DirectoryInfo dir = new DirectoryInfo(folderPath);
    FileInfo[] file = dir.GetFiles("*.*");
    public List<FilePath> notesPath = new List<FilePath>();
    public List<string> songName = new List<string>();
    FilePath tempPath;

    bool startReading;
    bool readingName;
    int column;
    string tempString;
    SpawnNote tempNote;

    // Start is called before the first frame update
    void Start()
    {
        if(singleton == null) singleton = this;
        else Destroy(gameObject);

        ReadNoteFiles();
    }

    public void ReadNoteFiles()
    {
        file = dir.GetFiles("*.*");
        notesPath.Clear();
        foreach (FileInfo f in file)
        {
            string[] nameWithoutExtension = f.Name.Split('.');

            tempPath.name = nameWithoutExtension[0];
            tempPath.path = folderPath + f.Name;
            notesPath.Add(tempPath);
        }
        ReadAllName();
    }

    public void ReadAllName()
    {
        songName.Clear();

        foreach (FilePath notepath in notesPath)
        {
            StreamReader reader = new StreamReader(notepath.path);
            string fullString = reader.ReadToEnd();
            reader.Close();

            for (int i = 0; fullString[i] != '$'; i++)
            {
                if (fullString[i] == '=')
                {
                    readingName = true;
                    continue;
                }

                if (readingName)
                {
                    tempString += fullString[i];
                    continue;
                }
            }
            songName.Add(tempString);
            tempString = "";
            readingName = false;
        }
    }

    public void WriteNew(string fileName)
    {
        tempPath.name = fileName;
        tempPath.path = folderPath + fileName + ".txt";
        notesPath.Add(tempPath);
        Write(notesPath.Count - 1);
    }

    public void Write(int index)
    {
        File.WriteAllText(notesPath[index].path, ""); // clear file before write
        StreamWriter writer = new StreamWriter(notesPath[index].path, true); // init stream writer
        string tempString;
        writer.WriteLine("Song Name=" + AudioManager.singleton.CurrentlySelectedSongName() + "$");
        writer.WriteLine("");
        writer.WriteLine("// POSITION X :  -7  to   7");
        writer.WriteLine("// POSITION Y : 3.2 to -3.2");
        writer.WriteLine("//            1           2          3           4           5            6         7");
        writer.WriteLine("// CHAIN: singleNote, upperLeft, upperRight, bottomLeft, bottomRight, leftRight, upDown");
        writer.WriteLine("//            1    2     3      4");
        writer.WriteLine("// DIRECTION: up, down, left, right");
        writer.WriteLine("// ");
        writer.WriteLine("// FORMAT: time~chain|direction,direction2|positionX,positionY|position2X,position2Y|");
        writer.WriteLine("*");
        foreach (SpawnNote tempNote in GameManager.singleton.spawnNote)
        {
            tempString = "";

            tempString += Math.Round(tempNote.time, 4).ToString("F4");
            tempString += "~";
            tempString += tempNote.chainCondition.ToString();
            tempString += "|";
            tempString += tempNote.direction.ToString();
            tempString += ",";
            tempString += tempNote.direction2.ToString();
            tempString += "|";
            tempString += Math.Round(tempNote.position.x, 2).ToString();
            tempString += ",";
            tempString += Math.Round(tempNote.position.y, 2).ToString();
            tempString += "|";
            tempString += Math.Round(tempNote.position2.x, 2).ToString();
            tempString += ",";
            tempString += Math.Round(tempNote.position2.y, 2).ToString();
            tempString += "|;";

            writer.WriteLine(tempString);
        }
        writer.WriteLine("#");
        writer.Close();
    }

    public void Read(int index)
    {
        // read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(notesPath[index].path);
        string fullString = reader.ReadToEnd();
        reader.Close();

        // reset values
        startReading = false;
        readingName = false;
        column = 0;
        tempString = "";
        GameManager.singleton.spawnNote.Clear(); // clear spawnNote before reading

        // assign the value into spawnNote according to the string
        for (int i = 0; fullString[i] != '#'; i++)
        {
            if (!startReading)
            { // haven't start yet
                if (fullString[i] == '*')
                { // fullString[i] is a starting character
                    i++;
                    startReading = true;
                    continue;
                }
                else
                {
                    continue;
                }
            }
            else
            { // start reading ady
                if (fullString[i] == '|' || fullString[i] == ',' || fullString[i] == '~')
                { // fullString[i] is a seperate character
                    SwitchColumn(tempString);
                    column++;
                    tempString = "";
                    continue;
                }
                else if (fullString[i] == ';')
                { // fullString[i] is a next line character
                    tempNote.isDisplaying = false;
                    tempNote.absoluteKill = true;
                    GameManager.singleton.spawnNote.Add(tempNote);
                    i++;
                    column = 0;
                    tempString = "";
                    continue;
                }
                else 
                {
                    tempString += fullString[i];
                }
            }
        }
    }

    void SwitchColumn(string tempString)
    {
        switch (column)
        {
            case 0:
                tempNote.time = float.Parse(tempString);
                break;
            case 1:
                tempNote.chainCondition = int.Parse(tempString);
                break;
            case 2:
                tempNote.direction = int.Parse(tempString);
                break;
            case 3:
                tempNote.direction2 = int.Parse(tempString);
                break;
            case 4:
                tempNote.position.x = float.Parse(tempString);
                break;
            case 5:
                tempNote.position.y = float.Parse(tempString);
                break;
            case 6:
                tempNote.position2.x = float.Parse(tempString);
                break;
            case 7:
                tempNote.position2.y = float.Parse(tempString);
                break;
        }
    }
}
