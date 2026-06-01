namespace Tests_and_Interviews_API.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    public interface ICompanyRepo
    {
        public void PrintAll();
        ObservableCollection<Company> GetAll();
        Company? GetById(int companyId);
        void Add(Company c);
        void Remove(int companyID);
        Company? GetCompanyByName(string companyName);
        void Update(Company c);
        Game? GetGame();
        void SaveGame(Game game);
        List<Company> GetByRecruiter(int recruiterId);

    }
}
