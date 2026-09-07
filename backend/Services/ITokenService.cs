using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}

public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
