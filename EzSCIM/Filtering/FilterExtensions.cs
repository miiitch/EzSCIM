using EzSCIM.Filtering.AST;
using EzSCIM.Models;

namespace EzSCIM.Filtering;

/// <summary>
/// Extension methods for applying FilterExpression to IEnumerable collections
/// </summary>
public static class FilterExtensions
{
    #region User Filtering Extensions

    /// <summary>
    /// Filters users based on a FilterExpression
    /// </summary>
    public static IEnumerable<ScimUser> Where(this IEnumerable<ScimUser> users, FilterExpression filter)
    {
        return filter switch
        {
            ComparisonFilter comp => users.WhereComparison(comp),
            PresenceFilter pres => users.WherePresence(pres),
            AndFilter and => users.WhereAnd(and),
            OrFilter or => users.WhereOr(or),
            NotFilter not => users.WhereNot(not),
            _ => users
        };
    }

    private static IEnumerable<ScimUser> WhereComparison(this IEnumerable<ScimUser> users, ComparisonFilter comp)
    {
        var value = comp.Value.GetStringValue();

        return comp.AttributeName.ToLower() switch
        {
            "username" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.UserName.Equals(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.NotEquals => users.Where(u => !u.UserName.Equals(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.Contains => users.Where(u => u.UserName.Contains(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.StartsWith => users.Where(u => u.UserName.StartsWith(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.EndsWith => users.Where(u => u.UserName.EndsWith(value, StringComparison.OrdinalIgnoreCase)),
                _ => users
            },
            "externalid" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.ExternalId?.Equals(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.StartsWith => users.Where(u => u.ExternalId?.StartsWith(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.Contains => users.Where(u => u.ExternalId?.Contains(value, StringComparison.OrdinalIgnoreCase) == true),
                _ => users
            },
            "displayname" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.DisplayName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.Contains => users.Where(u => u.DisplayName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.StartsWith => users.Where(u => u.DisplayName?.StartsWith(value, StringComparison.OrdinalIgnoreCase) == true),
                _ => users
            },
            "active" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.Active == bool.Parse(value)),
                FilterOperator.NotEquals => users.Where(u => u.Active != bool.Parse(value)),
                _ => users
            },
            "name.givenname" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.Name.GivenName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.Contains => users.Where(u => u.Name.GivenName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.StartsWith => users.Where(u => u.Name.GivenName?.StartsWith(value, StringComparison.OrdinalIgnoreCase) == true),
                _ => users
            },
            "name.familyname" => comp.Operator switch
            {
                FilterOperator.Equals => users.Where(u => u.Name.FamilyName?.Equals(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.Contains => users.Where(u => u.Name.FamilyName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true),
                FilterOperator.StartsWith => users.Where(u => u.Name.FamilyName?.StartsWith(value, StringComparison.OrdinalIgnoreCase) == true),
                _ => users
            },
            _ => users
        };
    }

    private static IEnumerable<ScimUser> WherePresence(this IEnumerable<ScimUser> users, PresenceFilter pres)
    {
        return pres.AttributeName.ToLower() switch
        {
            "username" => users.Where(u => !string.IsNullOrEmpty(u.UserName)),
            "displayname" => users.Where(u => !string.IsNullOrEmpty(u.DisplayName)),
            "externalid" => users.Where(u => !string.IsNullOrEmpty(u.ExternalId)),
            "name.givenname" => users.Where(u => !string.IsNullOrEmpty(u.Name.GivenName)),
            "name.familyname" => users.Where(u => !string.IsNullOrEmpty(u.Name.FamilyName)),
            _ => users
        };
    }

    private static IEnumerable<ScimUser> WhereAnd(this IEnumerable<ScimUser> users, AndFilter and)
    {
        var result = users.Where(and.Left);
        return result.Where(and.Right);
    }

    private static IEnumerable<ScimUser> WhereOr(this IEnumerable<ScimUser> users, OrFilter or)
    {
        var usersList = users.ToList();
        var left = usersList.Where(or.Left).ToList();
        var right = usersList.Where(or.Right).ToList();
        return left.Union(right);
    }

    private static IEnumerable<ScimUser> WhereNot(this IEnumerable<ScimUser> users, NotFilter not)
    {
        var usersList = users.ToList();
        var filtered = usersList.Where(not.Expression).ToList();
        return usersList.Where(u => !filtered.Contains(u));
    }

    #endregion

    #region Group Filtering Extensions

    /// <summary>
    /// Filters groups based on a FilterExpression
    /// </summary>
    public static IEnumerable<ScimGroup> Where(this IEnumerable<ScimGroup> groups, FilterExpression filter)
    {
        return filter switch
        {
            ComparisonFilter comp => groups.WhereComparison(comp),
            PresenceFilter pres => groups.WherePresence(pres),
            AndFilter and => groups.WhereAnd(and),
            OrFilter or => groups.WhereOr(or),
            NotFilter not => groups.WhereNot(not),
            _ => groups
        };
    }

    private static IEnumerable<ScimGroup> WhereComparison(this IEnumerable<ScimGroup> groups, ComparisonFilter comp)
    {
        var value = comp.Value.GetStringValue();

        return comp.AttributeName.ToLower() switch
        {
            "displayname" => comp.Operator switch
            {
                FilterOperator.Equals => groups.Where(g => g.DisplayName.Equals(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.Contains => groups.Where(g => g.DisplayName.Contains(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.StartsWith => groups.Where(g => g.DisplayName.StartsWith(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.EndsWith => groups.Where(g => g.DisplayName.EndsWith(value, StringComparison.OrdinalIgnoreCase)),
                _ => groups
            },
            "externalid" => comp.Operator switch
            {
                FilterOperator.Equals => groups.Where(g => g.ExternalId.Equals(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.Contains => groups.Where(g => g.ExternalId.Contains(value, StringComparison.OrdinalIgnoreCase)),
                FilterOperator.StartsWith => groups.Where(g => g.ExternalId.StartsWith(value, StringComparison.OrdinalIgnoreCase)),
                _ => groups
            },
            _ => groups
        };
    }

    private static IEnumerable<ScimGroup> WherePresence(this IEnumerable<ScimGroup> groups, PresenceFilter pres)
    {
        return pres.AttributeName.ToLower() switch
        {
            "displayname" => groups.Where(g => !string.IsNullOrEmpty(g.DisplayName)),
            "externalid" => groups.Where(g => !string.IsNullOrEmpty(g.ExternalId)),
            _ => groups
        };
    }

    private static IEnumerable<ScimGroup> WhereAnd(this IEnumerable<ScimGroup> groups, AndFilter and)
    {
        var result = groups.Where(and.Left);
        return result.Where(and.Right);
    }

    private static IEnumerable<ScimGroup> WhereOr(this IEnumerable<ScimGroup> groups, OrFilter or)
    {
        var groupsList = groups.ToList();
        var left = groupsList.Where(or.Left).ToList();
        var right = groupsList.Where(or.Right).ToList();
        return left.Union(right);
    }

    private static IEnumerable<ScimGroup> WhereNot(this IEnumerable<ScimGroup> groups, NotFilter not)
    {
        var groupsList = groups.ToList();
        var filtered = groupsList.Where(not.Expression).ToList();
        return groupsList.Where(g => !filtered.Contains(g));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extracts string value from FilterValue objects
    /// </summary>
    private static string GetStringValue(this FilterValue value)
    {
        return value switch
        {
            StringValue sv => sv.Value,
            BooleanValue bv => bv.Value.ToString().ToLowerInvariant(),
            NumericValue nv => nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DateTimeValue dv => dv.Value.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            _ => string.Empty
        };
    }

    #endregion
}
