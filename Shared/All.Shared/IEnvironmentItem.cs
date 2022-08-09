namespace All.Shared
{
    public interface IEnvironmentItem
    {
        string Name { get; set; }
        string AdminApiUrl { get; set; }
        string AdminAuth_Authority { get; set; }
        string AdminAuth_ClientId { get; set; }
        string AdminCertDomain { get; set; }
        string AdminUrl { get; set; }
        string ClientApiUrl { get; set; }
        string ClientAuth_Authority { get; set; }
        string ClientAuth_ClientId { get; set; }
        string ClientCertDomain { get; set; }
        string ClientUrl { get; set; }

        object Clone();
    }
}