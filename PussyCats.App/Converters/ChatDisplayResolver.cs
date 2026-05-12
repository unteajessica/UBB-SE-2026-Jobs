using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Converters;

internal static class ChatDisplayResolver
{
    private const int MissingSenderId = 0;
    private const string DefaultChatName = "Chat";

    public static string ResolveChatName(Chat chat)
    {
        if (chat.SecondUser is not null)
        {
            return chat.SecondUser.Name;
        }

        var session = App.Services.GetService(typeof(SessionContext)) as SessionContext;
        if (session is null)
        {
            return DefaultChatName;
        }

        if (session.Mode == AppMode.Company)
        {
            return $"User {chat.User.UserId}";
        }

        if (chat.CompanyId.HasValue)
        {
            var companyId = chat.CompanyId.Value;
            return $"Company {companyId}";
        }

        if (chat.SecondUser != null)
        {
            var currentUserId = session.UserId;
            return chat.User.UserId == currentUserId ? chat.SecondUser.Name : chat.User.Name;
        }

        return DefaultChatName;
    }

    public static int GetCurrentSenderId()
    {
        var session = App.Services.GetService(typeof(SessionContext)) as SessionContext;
        if (session is null)
        {
            return MissingSenderId;
        }

        return session.Mode switch
        {
            AppMode.Company => session.CompanyId ?? MissingSenderId,
            AppMode.Developer => session.DeveloperId ?? MissingSenderId,
            _ => session.UserId,
        };
    }
}
