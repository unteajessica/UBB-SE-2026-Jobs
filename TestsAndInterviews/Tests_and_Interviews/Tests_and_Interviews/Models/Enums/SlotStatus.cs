namespace Tests_and_Interviews.Models.Enums
{
    /// <summary>
    /// Specifies the status of a slot.
    /// </summary>
    public enum SlotStatus
    {
        /// <summary>
        /// Indicates that the slot is free and not occupied by a user.
        /// </summary>
        Free,

        /// <summary>
        /// Indicates that the slot is currently occupied/reserved by a user.
        /// </summary>
        Occupied,
    }
}