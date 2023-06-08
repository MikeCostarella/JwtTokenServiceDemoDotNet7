namespace JwtTokenService.DataAccess.Models
{
    public class Login
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime LoginDateTime { get; set; }
    }
}
