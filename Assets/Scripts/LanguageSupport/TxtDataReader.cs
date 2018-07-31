//----------------------------------------------
//            Excel2Tabel kit
// Copyright © 2015-2016 DD&YY Studio
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Read txt file's data
/// </summary>
abstract public class TxtDataReader {

    public static string EncryptKey = "D9ASDFKNMWEOIUDSALKMNVXZCVKJ";

    protected List<List<string>> ReaderData(string path, bool isEncrypt)
    {
        List<List<string>> text = new List<List<string>>();
        string encodeText = "";
        string decodeText = "";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_IPHONE
		StreamReader sr = File.OpenText(Application.dataPath + path);
        decodeText = sr.ReadToEnd();
#elif UNITY_ANDROID
        WWW www = new WWW("jar:file://" + Application.dataPath + path);
        while (!www.isDone) { }
        decodeText = www.text;
#endif
        if (isEncrypt)
            for (int i = 0; i < decodeText.Length; i++)
                encodeText += (char)(decodeText[i] ^ EncryptKey[i % EncryptKey.Length]);
        else
            encodeText = decodeText;

        encodeText = encodeText.Replace("\r\n", "\n");
        string[] textLines = encodeText.Split('\n');
        int count = 0;
        foreach (string str in textLines)
        {
            if (count < 3)
            {
                count++;
                continue;
            }
            if (string.IsNullOrEmpty(str))
            {
                return text;
            }
            string[] strs = str.Split('\t');
            List<string> lineStr = new List<string>();
            for (int i = 0; i < strs.Length; i++)
            {
                lineStr.Add(strs[i]);
            }
            text.Add(lineStr);
        }

        return text;
    }
}
