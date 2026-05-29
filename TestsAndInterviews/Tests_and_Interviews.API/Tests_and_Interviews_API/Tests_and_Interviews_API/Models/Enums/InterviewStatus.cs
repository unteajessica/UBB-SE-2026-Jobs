// <copyright file="InterviewStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Models.Enums
{
    /// <summary>
    /// Represents the possible statuses of an interview session.
    /// </summary>
    public enum InterviewStatus
    {
        /// <summary>
        /// The interview has been scheduled but has not yet started.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The interview is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The interview has been completed.
        /// </summary>
        Completed,
    }
}