using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class CSVWriter
{
    static string filename = "";

    public static void WriteToCSV(List<float> data, string s)
    {
        filename = Application.dataPath + "/data_" + s + ".csv";

        TextWriter tw = new StreamWriter(filename, true);

        for (int i = 0; i < data.Count; i++)
        {
            tw.WriteLine(i + "," + data[i]);
        }

        tw.Close();
    }
}
