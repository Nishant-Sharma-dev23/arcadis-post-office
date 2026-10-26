namespace Postal.Api.Domain;

public class PostOffice
{
    public int Id { get; private set; }
    public string ZipCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private PostOffice() { } // for EF Core

    public PostOffice(string zipCode, string name) => Update(zipCode, name);

    public void Update(string zipCode, string name)
    {
        ZipCode = zipCode.Trim();
        Name = name.Trim();
    }
}
