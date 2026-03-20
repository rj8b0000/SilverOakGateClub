using SilverOakGateClub.Models;

namespace SilverOakGateClub.Repository;

public interface ITestRepository
{
    // MockTests
    Task<MockTest?> GetTestByIdAsync(int id);
    Task<MockTest?> GetTestWithQuestionsAsync(int id);
    Task<List<MockTest>> GetTestsByBranchAsync(int branchId, bool isPYQ = false);
    Task<List<MockTest>> GetAllTestsAsync();
    Task<MockTest> CreateTestAsync(MockTest test);
    Task<MockTest> UpdateTestAsync(MockTest test);
    Task DeleteTestAsync(int id);

    // Questions
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<Question>> GetQuestionsByTestAsync(int testId);
    Task<Question> CreateQuestionAsync(Question question);
    Task<Question> UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(int id);
    Task AddQuestionsRangeAsync(IEnumerable<Question> questions);

    // TestResults
    Task<TestResult> SaveResultAsync(TestResult result);
    Task<List<TestResult>> GetResultsByUserAsync(int userId);
    Task<List<TestResult>> GetResultsByTestAsync(int testId);
    Task<TestResult?> GetResultByIdAsync(int id);
    Task<List<TestResult>> GetTopResultsAsync(int testId, int top = 10);
    Task<double> GetAverageScoreAsync(int testId);

    // PYQs
    Task<List<MockTest>> GetPYQsByBranchAsync(int branchId);

    // Stats
    Task<int> GetTotalTestsCountAsync();
    Task<int> GetTotalQuestionsCountAsync();
}
