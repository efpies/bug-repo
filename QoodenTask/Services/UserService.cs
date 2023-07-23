using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class UserService : IUserService
{
    private AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User> GetById(int id, bool includeBlocked = false)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id && includeBlocked ? includeBlocked : u.IsActive);
    }

    public async Task<List<User>> GetAll()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<User> Create(UserVM userVm)
    {
        var newUser = new User
        {
            UserName = userVm.UserName,
            Password = userVm.Password
        };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        return newUser;
    }

    public async void Update(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async void Block(int userId)
    {
        ChangingActivity(userId,false);
    }
    
    public async void Unblock(int userId)
    {
        ChangingActivity(userId,true);
    }
    
    public async void ChangingActivity(int UserId, bool IsActive)
    {
        var user = await GetById(UserId, true);
        if (user != null)
        {
            user.IsActive = IsActive;
            Update(user);
        }
    }
}