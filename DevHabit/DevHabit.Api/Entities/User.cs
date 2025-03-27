﻿namespace DevHabit.Api.Entities;

public sealed class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// We'll use this to store the Identity ID from the Identity Provider.
    /// This could be any identity provider like Azure AD, Okta, Auth0, etc.
    /// </summary>
    public string IdentityId { get; set; }
}
