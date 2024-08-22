using Fina.Api.Data;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class CategoryHandler(AppDbContext context) : ICategoryHandler
{
    private readonly AppDbContext _context = context;
    
    
    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        try
        {
            var category = new Category()
            {
                UserId = request.UserId,
                Description = request.Description,
                Title = request.Title
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return new Response<Category?>(category, 201, "Categoria Criada");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            var category = await _context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            category.Title = request.Title;
            category.Description = request.Description;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return new Response<Category?>(category, message: "Categoria atualizada com sucesso");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        try
        {
            var category = await _context
                .Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
            
            if (category is null)
                return new Response<Category?>(null, 404, "Categoria não encontrada");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            
            return new Response<Category?>(category, message: "Categoria removida com sucesso");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Response<Category?>> GetByIdAsync(GetCategoryByIdRequest request)
    {
        try
        {
            await Task.Delay(5000);
            var category = await _context
                .Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return category is null
                ? new Response<Category?>(null, 404, "Categoria não encontrada")
                : new Response<Category?>(category);
        }
        catch
        {
            return new Response<Category?>(null, 500, "Categoria não encontrada");
        }
    }

    public async Task<PagedResponse<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request)
    {
        try
        {
            var query = _context
                .Categories
                .AsNoTracking()
                .Where(x => x.UserId == request.UserId)
                .OrderBy(x => x.Title);

            var categories = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Category>?>(categories, count, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Category>?>(null, code: 500);
        }
    }
}