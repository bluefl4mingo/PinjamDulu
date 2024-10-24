namespace PinjamDuluApp.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Contact { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}