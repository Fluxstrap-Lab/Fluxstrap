namespace Fluxstrap.Models.Persistable
{
    public class ClientSettingsSaves
    {
        public string Name { get; set; } = "";

        public Dictionary<string, string> Flags { get; set; } = new();
    }
}
