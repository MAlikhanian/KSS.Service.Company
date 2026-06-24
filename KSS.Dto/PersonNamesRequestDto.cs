namespace KSS.Dto
{
    /// <summary>
    /// Request body sent to the Person service's POST /Api/Person/Names
    /// endpoint to resolve display names for a specific set of person ids.
    /// </summary>
    public class PersonNamesRequestDto
    {
        public List<Guid> Ids { get; set; } = new();
        public short LanguageId { get; set; } = 12;
    }
}
