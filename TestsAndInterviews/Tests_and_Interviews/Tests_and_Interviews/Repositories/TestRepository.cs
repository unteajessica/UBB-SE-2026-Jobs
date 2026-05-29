// <copyright file="TestRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Repositories.Interfaces;

    /// <summary>
    /// TestRepository class provides methods to perform CRUD operations on the Tests and Questions tables in the database.
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRepository"/> class.
        /// </summary>
        public TestRepository()
        {
            this.appDbContext = new AppDbContext(); // TODO - should be injectable
        }

        /// <summary>
        /// Asynchronously finds a test by its ID, including its associated questions.
        /// </summary>
        /// <param name="id">The ID of the test to find.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Test?> FindByIdAsync(int id)
        {
            return await this.appDbContext.Tests
                .Include(test => test.Questions)
                .FirstOrDefaultAsync(test => test.Id == id);
        }

        /// <summary>
        /// Asynchronously finds tests by their category, including their associated questions.
        /// </summary>
        /// <param name="category">The category of the tests to find.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<Test>> FindTestsByCategoryAsync(string category)
        {
            return await this.appDbContext.Tests
                .Include(test => test.Questions)
                .Where(test => test.Category == category)
                .ToListAsync();
        }
    }
}