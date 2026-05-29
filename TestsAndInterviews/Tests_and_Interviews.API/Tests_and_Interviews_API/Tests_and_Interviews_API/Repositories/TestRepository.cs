// <copyright file="TestRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// TestRepository class provides methods to perform CRUD operations on the Tests and Questions tables in the database.
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRepository"/> class.
        /// </summary>
        public TestRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <summary>
        /// Asynchronously retrieves all test entities from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all tests. The list is
        /// empty if no tests are found.</returns>
        public async Task<List<Test>> GetTestsASync()
        {
            return await this.appDbContext.Tests.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a list of all question category names associated with tests.
        /// </summary>
        /// <returns>A list of strings containing the names of all categories for questions that are linked to a test. The list
        /// will be empty if no such categories exist.</returns>
        public async Task<List<string>> GetAllCategories()
        {
            return await this.appDbContext.Tests
                .Select(t => t.Category)
                .Distinct()
                .ToListAsync();
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

        /// <summary>
        /// Asynchronously adds a new Test entity to the data store.
        /// </summary>
        /// <param name="test">The Test entity to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public async Task AddAsync(Test test)
        {
            this.appDbContext.Tests.Add(test);
            await this.appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously updates the specified test entity in the database.
        /// </summary>
        /// <param name="test">The test entity containing updated values. The entity's Id must correspond to an existing test in the
        /// database.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified Id does not exist in the database.</exception>
        public async Task UpdateAsync(Test test)
        {
            var existing = await this.appDbContext.Tests.FindAsync(test.Id);
            if (existing == null) {
                throw new KeyNotFoundException("Test not found");
            }

            existing.Title = test.Title;
            existing.Category = test.Category;
            await this.appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously deletes the test entity with the specified identifier from the data store.
        /// </summary>
        /// <param name="id">The unique identifier of the test entity to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test entity with the specified identifier does not exist.</exception>
        public async Task DeleteAsync(int id)
        {
            var existing = await this.appDbContext.Tests.FindAsync(id);
            if (existing == null) {
                throw new KeyNotFoundException("Test not found");
            }

            this.appDbContext.Tests.Remove(existing);
            await this.appDbContext.SaveChangesAsync();
        }
    }
}