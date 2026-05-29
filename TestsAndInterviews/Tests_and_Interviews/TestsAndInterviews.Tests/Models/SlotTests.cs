using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Enums;

namespace TestsAndInterviews.Tests.Models
{
	public class SlotTests
	{
		[Theory]
		[InlineData(12, 0, 16, 30, "12:00 - 16:30")]
		[InlineData(12, 23, 16, 58, "12:23 - 16:58")]
		public void TimeRange_ComputesStringCorrectly(int startTimeHours, int startTimeMinutes, int endTimeHours, int endTimeMinutes, string expectedTimeRange)
		{
			var slot = new Slot
			{
				StartTime = new DateTime(2026, 04, 22, startTimeHours, startTimeMinutes, 0),
				EndTime = new DateTime(2026, 04, 22, endTimeHours, endTimeMinutes, 0),
			};

			Assert.Equal(expectedTimeRange, slot.TimeRange);
		}

		[Theory]
		[InlineData(30, 1)]
		[InlineData(100, 3)]
		[InlineData(0, 1)]
		[InlineData(-1, 1)]
		public void RowSpan_PositiveDuration_ComputesNumberOfHalfHourBlocks(int duration, int expectedRowSpan)
		{
			var slot = new Slot
			{
				Duration = duration
			};

			Assert.Equal(expectedRowSpan, slot.RowSpan);
		}

		[Fact]
		public void Release_OccupiedSlot_SetsSlotFree()
		{
			var slot = new Slot
			{
				Status = SlotStatus.Occupied
			};

			slot.Release();

			Assert.Equal(SlotStatus.Free, slot.Status);
		}

		[Fact]
		public void Release_OccupiedSlot_SetsCandidateIdToZero()
		{
			var slot = new Slot
			{
				Status = SlotStatus.Occupied
			};

			slot.Release();

            Assert.Null(slot.CandidateId);
        }
	}
}
