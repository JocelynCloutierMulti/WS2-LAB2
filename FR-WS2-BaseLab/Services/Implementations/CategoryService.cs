using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;

namespace FR_WS2_BaseLab.Services.Implementations;
    public class CategoryService : ICategoryService
    {
        private readonly FrWs2BaselabContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
             FrWs2BaselabContext context,
             ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }
         public Task<ServiceResult<List<Category>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Category>> CreateAsync(Category category)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Category>> UpdateAsync(int id, Category category)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Category>> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
