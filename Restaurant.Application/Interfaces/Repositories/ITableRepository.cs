using System;
using System.Collections.Generic;
using System.Text;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Repositories
{
    public interface ITableRepository
    {
        Task<IEnumerable<Table>> GetAllAsync();
        Task<Table> GetByIdAsync(int id);
        Task AddAsync(Table table);
        Task UpdateAsync(Table table);
        Task DeleteAsync(int id);
    }
}
