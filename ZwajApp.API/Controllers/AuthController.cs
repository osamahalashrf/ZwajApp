using Microsoft.AspNetCore.Mvc;
using ZwajApp.API.Data;
using ZwajApp.API.Dtos;
using ZwajApp.API.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZwajApp.API.Services;


namespace ZwajApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repository;

        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // 1. تنظيف اسم المستخدم والتحقق من وجوده مسبقاً
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repository.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            // 2. تحويل الـ DTO إلى Model (User)
            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            // 3. حفظ المستخدم في قاعدة البيانات
            var createdUser = await _repository.Register(userToCreate, userForRegisterDto.Password);

            // 4. إنشاء التوكن للمستخدم الجديد (اللمسة الاحترافية)
            var jwtService = new JwtService(_config);
            var token = jwtService.CreateToken(createdUser);

            // 5. إرجاع النتيجة مع التوكن
            // نستخدم Ok بدلاً من Created حالياً لتسهيل استلام التوكن في الفرونت إند
            return Ok(new
            {
                token = token,
                user = createdUser.Username // اختياري: إرجاع اسم المستخدم للعرض
            });
        }

        // we will create Login method with username & password
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // تمرير الـ config للخدمة عند إنشائها
            var jwtService = new JwtService(_config);
            var token = jwtService.CreateToken(userFromRepo);

            return Ok(new { 
                token = token,
                user = userFromRepo.Username
                });
        }
    }
}