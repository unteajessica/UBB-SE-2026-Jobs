// <copyright file="CreateSlotDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Dtos
{
    /// <summary>
    /// Data transfer object for creating a recruiter slot.
    /// </summary>
    public class CreateSlotDto
    {
        /// <summary>Gets or sets the base slot containing start time and recruiter info.</summary>
        public SlotDto BaseSlot { get; set; } = new SlotDto();

        /// <summary>Gets or sets the duration of the slot in minutes.</summary>
        public int Duration { get; set; }
    }
}