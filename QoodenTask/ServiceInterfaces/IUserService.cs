using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface IUserService
{
    public Task<User> GetById(int id, bool includeBlocked = false);
    public Task<List<User>> GetAll();
    public Task<User> Create(UserVM userVm);
    public void Update(User user);
    public void Block(int userId);
    public void Unblock(int userId);
}