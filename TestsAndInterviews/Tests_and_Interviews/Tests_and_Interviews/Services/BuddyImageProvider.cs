using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Services
{
    public static class BuddyImageProvider
    {
        private const int FemaleImageKey = 0;
        private const int MaleImageKey = 1;
        private const int GenericPhotoKey = 0;

        private static readonly Dictionary<int, string> BuddyImages = new Dictionary<int, string>
        {
            { FemaleImageKey, "ms-appx:///Assets/AvatarFemale.png" },
            { MaleImageKey, "ms-appx:///Assets/AvatarMale.png" }
        };

        public static string GetImagePathById(int id)
        {
            if (BuddyImages.TryGetValue(id, out var path))
            {
                return path;
            }

            return BuddyImages[GenericPhotoKey];
        }
    }
}
