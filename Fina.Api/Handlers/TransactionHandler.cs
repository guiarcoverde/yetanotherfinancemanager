using Fina.Api.Data;
using Fina.Core.Common;
using Fina.Core.Enums;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Transactions;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    private readonly AppDbContext _context = context;
    
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        try
        {
            var transaction = new Transaction
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now,
                Amount = request.Amount,
                PaidOrReceivedAt = request.PaidOrReceivedAt,
                Title = request.Title,
                Type = request.Type
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 201, message: "Transação inserida com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, message: "Transação não pode ser inserida");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 })
            request.Amount *= -1;

        try
        {
            var transaction = await _context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Transação não encontrada");

            transaction.Title = request.Title;
            transaction.Amount = request.Amount;
            transaction.CategoryId = request.CategoryId;
            transaction.Type = request.Type;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, message: "Transação atualizada com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Transação não pode ser atualizada");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await _context
                .Transactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
            
            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Categoria não encontrada");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            
            return new Response<Transaction?>(transaction, message: "Categoria removida com sucesso");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await _context
                .Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return transaction is null
                ? new Response<Transaction?>(null, 404, "Categoria não encontrada")
                : new Response<Transaction?>(transaction);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Categoria não encontrada");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        request.StartDate ??= DateTime.Now.GetFirstDay();
        request.EndDate ??= DateTime.Now.GetLastDay();
        try
        {
            var query = _context
                .Transactions
                .AsNoTracking()
                .Where(x =>
                    x.PaidOrReceivedAt >= request.StartDate &&
                    x.PaidOrReceivedAt <= request.EndDate &&
                    x.UserId == request.UserId)
                .OrderBy(x => x.PaidOrReceivedAt);

            var transactions = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Transaction>?>(transactions, count, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Transaction>?>(null, code: 500,
                message: "Não foi possivel determinar a data");
        }
    }
}