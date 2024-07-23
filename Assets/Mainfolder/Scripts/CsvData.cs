using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class CsvData
{
    public float time { get; set; }
    public float force_x { get; set; }
    public float force_y { get; set; }
    public float force_z { get; set; }
}

public static class CsvReader
{
    public static List<CsvData> ReadCsv(string filePath)
    {
        var records = new List<CsvData>();
        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');
                if (values.Length == 7 && float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float time) &&
                    float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float force_x) &&
                    float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float force_y) &&
                    float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float force_z))
                {
                    var record = new CsvData
                    {
                        time = time,
                        force_x = force_x,
                        force_y = force_y,
                        force_z = force_z
                    };
                    records.Add(record);
                }
            }
        }
        return records;
    }
}

