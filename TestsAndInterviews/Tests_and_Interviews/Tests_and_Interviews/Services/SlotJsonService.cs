// <copyright file="SlotJsonService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using Tests_and_Interviews.Models;

    /// <summary>
    /// Service for loading and saving slot data to a JSON file.
    /// </summary>
    public static class SlotJsonService
    {
        private static string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "slots.json");

        /// <summary>
        /// Load slots from the JSON file. If the file does not exist, return an empty list.
        /// </summary>
        /// <returns>A list of slots loaded from the JSON file.</returns>
        public static List<Slot> LoadSlots()
        {
            if (!File.Exists(filePath))
            {
                return new List<Slot>();
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Slot>>(json) ?? new List<Slot>();
        }

        /// <summary>
        /// Save slots to the JSON file. This will overwrite any existing data in the file.
        /// </summary>
        /// <param name="slots">The list of slots to be saved to the JSON file.</param>
        public static void SaveSlots(List<Slot> slots)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            string json = JsonSerializer.Serialize(slots, options);
            File.WriteAllText(filePath, json);

            System.Diagnostics.Debug.WriteLine("JSON saved at: " + filePath);
        }
    }
}