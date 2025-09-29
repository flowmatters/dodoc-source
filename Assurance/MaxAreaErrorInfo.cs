namespace FlowMatters.Source.DODOC.Assurance;

/// <summary>
/// A class used to allow the DODOC models to communicate to the Assurance Rule that a problem has occurred.
/// </summary>
public class MaxAreaErrorInfo
{
    public bool ErrorLogged;
    public string Location;
    public double MaxArea;
    public double ModelledArea;
}