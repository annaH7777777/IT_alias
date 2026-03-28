using System.Collections.Generic;

/// <summary>
/// Proper CSV parser that handles quoted fields (e.g. "value with, comma").
/// </summary>
public static class CSVParser
{
    public static List<string> SplitLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        int fieldStart = 0;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(CleanField(line.Substring(fieldStart, i - fieldStart)));
                fieldStart = i + 1;
            }
        }

        // Last field
        fields.Add(CleanField(line.Substring(fieldStart)));
        return fields;
    }

    private static string CleanField(string field)
    {
        field = field.Trim();

        // Remove surrounding quotes and unescape doubled quotes
        if (field.Length >= 2 && field[0] == '"' && field[field.Length - 1] == '"')
        {
            field = field.Substring(1, field.Length - 2);
            field = field.Replace("\"\"", "\"");
        }

        return field;
    }
}
