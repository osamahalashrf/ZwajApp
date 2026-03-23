using System.ComponentModel.DataAnnotations;
namespace ZwajApp.API.Dtos
{

    public class UserForRegisterDto
    {
        [Required (ErrorMessage = "Username is required") ]
        public string Username { get; set; }
        [Required (ErrorMessage = "Password is required"), StringLength(10, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 10 characters") ]
        public string Password { get; set; }
    }
}