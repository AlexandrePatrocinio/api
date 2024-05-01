using AutoCRUD.Models;

namespace api.Models
{
    public class Person : IEntity
    {
        public Guid Id { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public string[]? Stack { get; set; }

        public Person()
        {
            Id = Guid.NewGuid();
        }

        public Person(string alias, string name, string birthdate)
        {
            Id = Guid.NewGuid();
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DateTime date; 
            if (DateTime.TryParse(birthdate, out date))
                Birthdate =  date;
            else 
                throw new ArgumentException(nameof(birthdate));
        }
    }
}