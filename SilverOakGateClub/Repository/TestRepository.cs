using Microsoft.EntityFrameworkCore;
using SilverOakGateClub.Data;
using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _context;

    public TestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // MockTests
    public async Task<MockTest?> GetTestByIdAsync(int id)
    {
        return await _context.MockTests
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<MockTest?> GetTestWithQuestionsAsync(int id)
    {
        return await _context.MockTests
            .Include(t => t.Branch)
            .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<MockTest>> GetTestsByBranchAsync(int branchId, bool isPYQ = false)
    {
        return await _context.MockTests
            .Where(t => t.BranchId == branchId && t.IsActive && t.IsPYQ == isPYQ)
            .Include(t => t.Branch)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MockTest>> GetAllTestsAsync()
    {
        return await _context.MockTests
            .Include(t => t.Branch)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<MockTest> CreateTestAsync(MockTest test)
    {
        _context.MockTests.Add(test);
        await _context.SaveChangesAsync();
        return test;
    }

    public async Task<MockTest> UpdateTestAsync(MockTest test)
    {
        _context.MockTests.Update(test);
        await _context.SaveChangesAsync();
        return test;
    }

    public async Task DeleteTestAsync(int id)
    {
        var test = await _context.MockTests.FindAsync(id);
        if (test != null)
        {
            _context.MockTests.Remove(test);
            await _context.SaveChangesAsync();
        }
    }

    // Questions
    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions.FindAsync(id);
    }

    public async Task<List<Question>> GetQuestionsByTestAsync(int testId)
    {
        return await _context.Questions
            .Where(q => q.MockTestId == testId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<Question> UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddQuestionsRangeAsync(IEnumerable<Question> questions)
    {
        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();
    }

    // TestResults
    public async Task<TestResult> SaveResultAsync(TestResult result)
    {
        _context.TestResults.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<List<TestResult>> GetResultsByUserAsync(int userId)
    {
        return await _context.TestResults
            .Where(r => r.UserId == userId)
            .Include(r => r.MockTest)
                .ThenInclude(m => m.Branch)
            .OrderByDescending(r => r.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<TestResult>> GetResultsByTestAsync(int testId)
    {
        return await _context.TestResults
            .Where(r => r.MockTestId == testId)
            .Include(r => r.User)
            .OrderByDescending(r => r.Score)
            .ToListAsync();
    }

    public async Task<TestResult?> GetResultByIdAsync(int id)
    {
        return await _context.TestResults
            .Include(r => r.MockTest)
                .ThenInclude(m => m.Branch)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<TestResult>> GetTopResultsAsync(int testId, int top = 10)
    {
        return await _context.TestResults
            .Where(r => r.MockTestId == testId)
            .Include(r => r.User)
            .OrderByDescending(r => r.Score)
            .Take(top)
            .ToListAsync();
    }

    public async Task<double> GetAverageScoreAsync(int testId)
    {
        var results = await _context.TestResults
            .Where(r => r.MockTestId == testId)
            .ToListAsync();
        return results.Count > 0 ? results.Average(r => r.Percentage) : 0;
    }

    // PYQs
    public async Task<List<MockTest>> GetPYQsByBranchAsync(int branchId)
    {
        return await _context.MockTests
            .Where(t => t.BranchId == branchId && t.IsPYQ && t.IsActive)
            .Include(t => t.Branch)
            .OrderByDescending(t => t.Year)
            .ToListAsync();
    }

    // Stats
    public async Task<int> GetTotalTestsCountAsync()
    {
        return await _context.MockTests.CountAsync(t => t.IsActive);
    }

    public async Task<int> GetTotalQuestionsCountAsync()
    {
        return await _context.Questions.CountAsync();
    }
}
