namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for retrieving questions by test or position.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access question data. Cannot be null.</param>
        public QuestionService(IQuestionRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Asynchronously retrieves all questions belonging to the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of questions for the specified test.</returns>
        public async Task<List<Question>> GetQuestionsByTestIdAsync(int testId)
        {
            return await this._repository.FindByTestIdAsync(testId);
        }

        /// <summary>
        /// Asynchronously retrieves all interview questions for the specified position.
        /// </summary>
        /// <param name="positionId">The unique identifier of the position.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of interview questions for the specified position.</returns>
        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            return await this._repository.GetInterviewQuestionsByPositionAsync(positionId);
        }
    }
}