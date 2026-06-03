namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _service;

        public SlotsController(ISlotService service)
        {
            this._service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SlotDto>> GetById(int id)
        {
            try
            {
                Slot slot = await this._service.GetSlotByIdAsync(id);
                return Ok(slot.ToDto());
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("recruiter/{recruiterId}")]
        public async Task<ActionResult<List<SlotDto>>> GetAllByRecruiter(int recruiterId)
        {
            List<Slot> slots = await this._service.GetAllSlotsAsync(recruiterId);

            return Ok(slots.Select(slot => slot.ToDto()).ToList());
        }

        [HttpGet("candidate/{candidateId}")]
        public async Task<ActionResult<List<SlotDto>>> GetByCandidate(int candidateId)
        {
            List<SlotDto> slots = await this._service.GetSlotsByCandidateAsync(candidateId);
            return Ok(slots);
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<SlotDto>>> GetAvailableByDate([FromQuery] DateTime date)
        {
            List<SlotDto> slots = await this._service.GetAvailableSlotsForDateAsync(date);
            return Ok(slots);
        }

        [HttpGet("recruiter/{recruiterId}/date")]
        public async Task<ActionResult<List<SlotDto>>> GetByRecruiter(int recruiterId, [FromQuery] DateTime date)
        {
            List<Slot> slots = await this._service.GetSlotsAsync(recruiterId, date);

            return Ok(slots.Select(slot => slot.ToDto()).ToList());
        }

        [HttpPost()]
        public async Task<ActionResult<SlotDto>> Create([FromBody] SlotDto dto)
        {
            try
            {
                Slot created = await this._service.AddSlotAsync(dto.ToEntity());
                return Ok(created);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SlotDto>> Update(int id, [FromBody] SlotDto dto)
        {
            try
            {
                Slot updated = await this._service.UpdateSlotAsync(id, dto.ToEntity());
                return Ok(updated);
            } catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await this._service.DeleteSlotAsync(id);

                if (deleted)
                {
                    return Ok(new { message = "Slot was deleted successfully" });
                }

                return BadRequest();
            } catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("recruiter/{recruiterId}/visible")]
        public async Task<ActionResult<List<SlotDto>>> GetVisibleSlots(int recruiterId, [FromQuery] DateTime date)
        {
            List<SlotDto> slots = await this._service.LoadRecruiterVisibleSlotsAsync(recruiterId, date);
            return Ok(slots);
        }

        [HttpPost("recruiter/create")]
        public async Task<ActionResult> CreateRecruiterSlot([FromBody] CreateSlotDto dto)
        {
            await this._service.CreateRecruiterSlotAsync(dto.BaseSlot, dto.Duration);
            return Ok();
        }

        [HttpPut("recruiter/update")]
        public async Task<ActionResult> UpdateRecruiterSlot([FromBody] UpdateSlotDto dto)
        {
            try
            {
                await this._service.UpdateRecruiterSlotAsync(dto.InitialSlot, dto.StartTime, dto.Duration);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
