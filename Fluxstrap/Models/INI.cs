namespace Fluxstrap.Models
{
    public static class INI
    {
        public static Dictionary<string, Dictionary<string, string>> Parse(string text)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            var currentSection = new Dictionary<string, string>();

            foreach (var line in text.Split('\n', '\r'))
            {
                var trimmed = line.Trim();

                if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                    continue;

                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    currentSection = new Dictionary<string, string>();
                    result[trimmed[1..^1]] = currentSection;
                    continue;
                }

                var equalsIndex = trimmed.IndexOf('=');

                if (equalsIndex < 0)
                    continue;

                var key = trimmed[..equalsIndex].Trim();
                var value = trimmed[(equalsIndex + 1)..].Trim();

                currentSection[key] = value;
            }

            return result;
        }

        public static string Write(Dictionary<string, Dictionary<string, string>> sections)
        {
            var sb = new StringBuilder();

            foreach (var (sectionName, keys) in sections)
            {
                sb.Append('[');
                sb.Append(sectionName);
                sb.AppendLine("]");

                foreach (var (key, value) in keys)
                {
                    sb.Append(key);
                    sb.Append('=');
                    sb.AppendLine(value);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
