namespace AppDomainMessaging
{
    public sealed class AppDomainNameRecord
    {
        public string Name { get; }

        public string Location { get; }

        public AppDomainNameRecord(string name, string location)
        {
            Name = name;
            Location = location;
        }
    }
}
