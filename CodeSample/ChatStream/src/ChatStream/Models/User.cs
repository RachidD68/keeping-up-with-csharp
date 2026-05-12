namespace ChatStream.Models;

/// <summary>A chat user with status and preferences.</summary>
/// <param name="Name">The user's display name.</param>
/// <param name="Status">Current online status.</param>
/// <param name="JoinedAt">When the user joined.</param>
public record User(string Name, UserStatus Status, DateTimeOffset JoinedAt)
{
    /// <summary>Creates a new online user.</summary>
    public static User Create(string name) =>
        new(name, UserStatus.Online, DateTimeOffset.UtcNow);
}

/// <summary>User online status.</summary>
public enum UserStatus { Offline, Online, Away, DoNotDisturb }
