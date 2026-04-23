using System;
using System.Collections.Generic;
using System.Text;
using Restaurant.Domain.Entities;   

namespace Restaurant.Application.Interfaces.Repositories
{
     public interface ICategoryRepository
    {
        Task<IEnumerable<Category>>GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

    }
}
