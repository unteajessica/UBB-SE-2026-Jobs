// <copyright file="InterviewSessionDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Web.Dtos
{
    using System;

    public class InterviewSessionDto
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int? ExternalUserId { get; set; }
        public int InterviewerId { get; set; }
        public DateTime DateStart { get; set; }
        public string? Video { get; set; }
        public string? Status { get; set; }
        public decimal? Score { get; set; }
    }
}