using System.Collections.ObjectModel;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    public class FakeGameRepository : ICompanyRepo
    {
        public Game StoredGame;
        public Game SavedGame;

        public Game GetGame()
        {
            return StoredGame;
        }

        public void SaveGame(Game game)
        {
            SavedGame = game;
        }
        public void Add(Company c)
        {
        }
        public void Remove(int id)
        {
        }
        public void Update(Company c)
        {
        }
        public Company GetById(int id)
        {
            return null;
        }

        public ObservableCollection<Company> GetAll()
        {
            return new ObservableCollection<Company>();
        }

        public Company GetCompanyByName(string name)
        {
            return null;
        }

        public void PrintAll()
        {
        }
    }
}