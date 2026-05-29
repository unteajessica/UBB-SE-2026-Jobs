// <copyright file="SlotDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Web.Dtos
{
    using System;

    public class SlotDto
    {
        public int Id { get; set; }
        public int RecruiterId { get; set; }
        public int? CandidateId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public int Status { get; set; } 
        public string InterviewType { get; set; } = string.Empty;
        public string TimeRange => $"{this.StartTime:HH:mm} - {this.EndTime:HH:mm}";
    }
}