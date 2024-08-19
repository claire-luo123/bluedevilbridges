namespace BlueDevilBridges.Objects;

public class User
{
	public string NetId { get; set; }
	public string FullName { get; set; }
	public bool IsAllowed { get; set; }

	public string FirstName => FullName?.Split(' ').FirstOrDefault();
}