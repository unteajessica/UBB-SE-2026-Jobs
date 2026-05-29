using PussyCats.Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace PussyCats.Library.Services
{
    public class Helpers
    {
        public static string GenerateParsedCvText(User user)
        {
            if (user is null)
            {
                return string.Empty;
            }

            var parsedCvTextBuilder = new StringBuilder();
            parsedCvTextBuilder.AppendLine($"{user.FirstName} {user.LastName}".Trim());
            parsedCvTextBuilder.AppendLine(user.University ?? string.Empty);
            parsedCvTextBuilder.AppendLine(string.Join(", ", user.Skills.Select(skill => skill.Skill?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name))));
            return parsedCvTextBuilder.ToString().TrimEnd();
        }
    }
}
