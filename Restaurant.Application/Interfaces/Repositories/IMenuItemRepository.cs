using System;
using System.Collections.Generic;
using System.Text;
using Restaurant.Domain.Entities;   

namespace Restaurant.Application.Interfaces.Repositories
{
    public interface IMenuItemRepository
    {
        Task<IEnumerable<MenuItem>> GetAllAsync();
        Task<MenuItem> GetByIdAsync(int id);
        Task AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(int id);
    }
}
