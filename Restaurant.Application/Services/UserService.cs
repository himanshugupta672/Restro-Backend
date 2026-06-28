using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Repositories;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IOrderRepository _orderRepository;

    public UserService(
        IUserRepository repository,
        IPasswordHashService passwordHashService,
        IOrderRepository orderRepository)
    {
        _repository = repository;
        _passwordHashService = passwordHashService;
        _orderRepository = orderRepository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<User>> GetChefsAsync()
    {
        return await _repository.GetChefsAsync();
    }

    public async Task<List<User>> GetAvailableChefsAsync()
    {
        var chefs = await _repository.GetAvailableChefsAsync();

        return chefs
            .Where(x => x.IsActive)
            .ToList();
    }

    public async Task SetAvailable(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Available;
        await _repository.UpdateAsync(user);
    }

    public async Task SetBusy(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Busy;
        await _repository.UpdateAsync(user);
    }

    public async Task SetOffline(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Offline;
        await _repository.UpdateAsync(user);
    }
    public async Task<User?> GetNextAvailableChefAsync()
    {
        var chefs = await _repository.GetAvailableChefsAsync();

        return chefs.FirstOrDefault();
    }
    public async Task RegisterChefAsync(CreateUserDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new Exception("Password and confirm password do not match");

        ValidateRole(dto.Role);

        var existing = await _repository.GetByEmailAsync(dto.Email);

        if (existing != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = NormalizeOptionalText(dto.PhoneNumber),
            Password = _passwordHashService.HashPassword(dto.Password),
            Address = NormalizeOptionalText(dto.Address),
            Role = (UserRole)dto.Role,
            Status = UserStatus.Available,
            LastAssignedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user); 
    }

    public async Task CreateByAdminAsync(CreateUserDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new Exception("Password and confirm password do not match");

        ValidateRole(dto.Role);

        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new Exception("Email is already in use");

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            var existingPhone = await _repository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null)
                throw new Exception("Phone number is already in use");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = NormalizeOptionalText(dto.PhoneNumber),
            Password = _passwordHashService.HashPassword(dto.Password),
            Address = NormalizeOptionalText(dto.Address),

            Role = (UserRole)dto.Role, // admin decides
            Status = UserStatus.Available,
            LastAssignedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return false;

        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null && existing.Id != id)
            throw new Exception("Email is already in use by another user");

        ValidateRole(dto.Role);

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            var existingPhone = await _repository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null && existingPhone.Id != id)
                throw new Exception("Phone number is already in use by another user");
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.PhoneNumber = NormalizeOptionalText(dto.PhoneNumber);
        user.Address = NormalizeOptionalText(dto.Address);
        user.Role = (UserRole)dto.Role;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.Password = _passwordHashService.HashPassword(dto.Password);
        }

        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ForgotPasswordDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new Exception("Password and confirm password do not match");

        var user = await _repository.GetByEmailAsync(dto.Email);

        if (user == null)
            return false;

        user.Password = _passwordHashService.HashPassword(dto.Password);
        await _repository.UpdateAsync(user);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user == null)
            return false;

        await _repository.DeleteAsync(user);

        return true;
    }

    public async Task<User> RegisterCustomerAsync(CustomerRegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new Exception("Password and confirm password do not match");

        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new Exception("Email is already registered");

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            var existingPhone = await _repository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null)
                throw new Exception("Phone number is already registered");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = NormalizeOptionalText(dto.PhoneNumber),
            Password = _passwordHashService.HashPassword(dto.Password),
            Role = UserRole.Customer,
            Status = UserStatus.Available,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(user);
        return user;
    }

    public async Task<List<CustomerDetailsDto>> GetCustomerDetailsAsync()
    {
        var customers = await _repository.GetCustomersAsync();
        var details = new List<CustomerDetailsDto>();

        foreach (var customer in customers)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
            var totalOrders = orders.Count;
            var totalSpent = orders.Sum(o => o.TotalAmount);
            var lastOrderDate = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt;

            details.Add(new CustomerDetailsDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                IsActive = customer.IsActive,
                CreatedDate = customer.CreatedDate,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                LastOrderDate = lastOrderDate
            });
        }

        return details;
    }

    public async Task<bool> ToggleUserActiveStatusAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _repository.UpdateAsync(user);
        return true;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateRole(int role)
    {
        if (!Enum.IsDefined(typeof(UserRole), role))
        {
            throw new Exception("Invalid user role");
        }
    }
}
