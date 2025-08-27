namespace API.Constants;

public static class PolicyConstants
{
    /// <summary>
    /// Grants user permission to create deliveries on another users behalf
    /// </summary>
    public const string CreateForOthers = nameof(CreateForOthers);
    /// <summary>
    /// Grants user permission to handle deliveries (move to handle state)
    /// </summary>
    public const string HandleDeliveries = nameof(HandleDeliveries);
    /// <summary>
    /// Grants user permission to view other users deliveries
    /// </summary>
    public const string ViewAllDeliveries  = nameof(ViewAllDeliveries);

    /// <summary>
    /// Grants user permission to manually update stock
    /// </summary>
    public const string ManageStock = nameof(ManageStock);
    /// <summary>
    /// Grants user permission to create, update, and delete products
    /// </summary>
    public const string ManageProducts = nameof(ManageProducts);
    /// <summary>
    /// Grants user permission to update and delete clients, everyone has access to create them
    /// </summary>
    public const string ManageClients = nameof(ManageClients);
    /// <summary>
    /// Grants user permission to change application settings
    /// </summary>
    public const string ManageApplication =  nameof(ManageApplication);

    public static IList<string> Roles = [CreateForOthers, HandleDeliveries, ViewAllDeliveries, ManageStock, ManageProducts, ManageClients, ManageApplication];
}