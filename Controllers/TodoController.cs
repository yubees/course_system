// Controllers/TodoController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkingwithSQLLiteinAsp.NETCoreWebAPI.ApplicationDbContext;
using WorkingwithSQLLiteinAsp.NETCoreWebAPI.Models;

[ApiController]
[Route("api/[controller]/[Action]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _jwtSecret;
    private readonly double _jwtExpirationInMinutes;

    public AuthController(AppDbContext context)
    {
        _context = context;
        _jwtSecret = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"; // Change this to a secure secret key
        _jwtExpirationInMinutes = 1440; // Token expiration time in minutes
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> AllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // // GET: api/Todo/5
    // [HttpGet("{id}")]
    // public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
    // {
    //     var todoItem = await _context.TodoItems.FindAsync(id);

    //     if(todoItem == null)
    //     {
    //         return NotFound();
    //     }

    //     return todoItem;
    // }

    // POST: api/Register
    [HttpPost]
    public async Task<ActionResult<User>> Register(User user)
    {
        // Check if user already exists
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            return BadRequest("User already exists.");
        }

        // Hash the password
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            user.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        //     var redirectUrl = "http://example.com/redirect";
        // return Redirect($"{redirectUrl}");

        return Ok("User created");
    }
    [HttpPost]
    public async Task<ActionResult<string>> Login(LoginRequest loginRequest)
    {
        // Hash the password
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(loginRequest.Password));
            string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            // Check if the user exists with the provided credentials
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username && u.Password == hashedPassword);

            if (user == null)
            {
                return BadRequest("Invalid username or password.");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    // Add more claims as needed, e.g., user roles, etc.
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(tokenString);
        }
    }

    // // PUT: api/Todo/5
    // [HttpPut("{id}")]
    // public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
    // {
    //     if(id != todoItem.Id)
    //     {
    //         return BadRequest();
    //     }

    //     _context.Entry(todoItem).State = EntityState.Modified;

    //     try
    //     {
    //         await _context.SaveChangesAsync();
    //     }
    //     catch(DbUpdateConcurrencyException)
    //     {
    //         if(!TodoItemExists(id))
    //         {
    //             return NotFound();
    //         }
    //         else
    //         {
    //             throw;
    //         }
    //     }

    //     return NoContent();
    // }

    // // DELETE: api/Todo/5
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteTodoItem(long id)
    // {
    //     var todoItem = await _context.TodoItems.FindAsync(id);
    //     if(todoItem == null)
    //     {
    //         return NotFound();
    //     }

    //     _context.TodoItems.Remove(todoItem);
    //     await _context.SaveChangesAsync();

    //     return NoContent();
    // }

    // private bool TodoItemExists(long id)
    // {
    //     return _context.TodoItems.Any(e => e.Id == id);
    // }
}