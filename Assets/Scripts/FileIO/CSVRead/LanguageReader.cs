using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LanguageReader:CSVReadBase{
public class LanguageData{
public string Chinese;
public string English;
public string Japanese;
}
public Dictionary<string,LanguageData> LanguageDatas=new Dictionary<string,LanguageData>();
public override void WriteData(int line, int row,string data)
{
if(row==0){if(!LanguageDatas.ContainsKey(message[line,row])){
LanguageDatas.Add(message[line,row],new LanguageData());
}
}
else{
if(1==row)LanguageDatas[message[line,0]].Chinese=GetValueResult<string>(message[line,row]);
if(2==row)LanguageDatas[message[line,0]].English= GetValueResult<string>(message[line,row]);
if(3==row)LanguageDatas[message[line,0]].Japanese= GetValueResult<string>(message[line,row]);
}
}
}
