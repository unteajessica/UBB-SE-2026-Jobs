namespace Tests_and_Interviews.Mappers
{
	using Tests_and_Interviews.Dtos;
	using Tests_and_Interviews.Models;

	/// <summary>
	/// Provides extension methods for mapping between Slot and SlotDto objects.
	/// </summary>
	public static class SlotMapper
	{
		/// <summary>
		/// Converts a SlotDto instance to a Slot entity.
		/// </summary>
		/// <param name="dto">The SlotDto to convert.</param>
		/// <returns>A Slot entity with values mapped from given SlotDto.</returns>
		public static Slot ToEntity(this SlotDto dto)
		{
			return new Slot
			{
				Id = dto.Id,
				RecruiterId = dto.RecruiterId,
				CandidateId = dto.CandidateId,
				StartTime = dto.StartTime,
				EndTime = dto.EndTime,
				Duration = dto.Duration,
				Status = dto.Status,
				InterviewType = dto.InterviewType,
			};
		}

		/// <summary>
		/// Converts a Slot entity to a SlotDto instance.
		/// </summary>
		/// <param name="entity">The Slot entity to convert.</param>
		/// <returns>A SlotDto with values mapped from given Slot entity.</returns>
		public static SlotDto ToDto(this Slot entity)
		{
			return new SlotDto
			{
				Id = entity.Id,
				RecruiterId = entity.RecruiterId,
				CandidateId = entity.CandidateId,
				StartTime = entity.StartTime,
				EndTime = entity.EndTime,
				Duration = entity.Duration,
				Status = entity.Status,
				InterviewType = entity.InterviewType,
			};
		}
	}
}
