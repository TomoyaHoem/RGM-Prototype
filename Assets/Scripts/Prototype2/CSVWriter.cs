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

    public static void WriteFitnessToCSV(List<List<float>> obj)
    {
        filename = Application.dataPath + "/fitness.csv";

        TextWriter tw = new StreamWriter(filename, true);

        for (int i = 0; i < obj.Count; i++)
        {
            tw.WriteLine(i + "," + obj[i][0] + "," + obj[i][1] + "," + obj[i][2] + "," + obj[i][3] + "," + obj[i][4]);
        }

        tw.Close();
    }

    public static void WriteEAResultsToCSV(List<List<float>> results)
    {
        filename = Application.dataPath + "/EAStatistics.csv";

        TextWriter tw = new StreamWriter(filename, true);

        for (int i = 0; i < results.Count; i++)
        {
            for (int j = 0; j < results[i].Count; j++)
            {
                if(j == results[i].Count-1)
                {
                    tw.Write(results[i][j]);
                    continue;
                }
                tw.Write(results[i][j] + ",");
            }
            tw.WriteLine();
        }

        tw.Close();
    }

    public static void WriteParetoDatatoCSV(List<GameObject> paretoFront)
    {
        filename = Application.dataPath + "/ParetoStatistics.csv";

        TextWriter tw = new StreamWriter(filename, true);

        int count = 0;

        for (int i = 0; i < paretoFront.Count; i++)
        {
            if (paretoFront[i] == null) continue;
            tw.Write(count + ",");
            count++;
            foreach (float f in paretoFront[i].GetComponent<Machine>().FitnessVals)
            {
                tw.Write(f + ",");
            }
            int[] distribution = { 0, 0, 0, 0, 0 };
            foreach (GameObject segment in paretoFront[i].GetComponent<Machine>().Segments)
            {
                int id = segment.GetComponent<SegmentPart>().SegmentID;
                if (id == 2 || id == 3)
                {
                    distribution[2]++;
                }
                else if (id > 3)
                {
                    distribution[id - 1]++;
                }
                else
                {
                    distribution[id]++;
                }
            }
            foreach (int num in distribution)
            {
                tw.Write(num + ",");
            }
            tw.Write(paretoFront[i].GetComponent<Machine>().Segments.Count);
            tw.WriteLine();
        }

        tw.Close();
    }
}
