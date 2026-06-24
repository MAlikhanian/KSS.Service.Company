namespace KSS.Dto
{
    /// <summary>
    /// Minimal person name record returned by the Person service's
    /// POST /Api/Person/Names endpoint. Used to resolve stakeholder
    /// related-party / board-representative display names.
    /// </summary>
    public class PersonNameDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
    }
}
