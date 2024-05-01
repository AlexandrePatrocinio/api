using AutoCRUD.Models;

namespace api.Models;

public class Companie : IEntity {
        public Guid Id { get; set; }

        public string TradeName { get; set; } = string.Empty;
        public string BusinessSector { get; set; } = string.Empty;

        public int NumberEmployees { get; set; }
        public DateTime FoundingDate { get; set; }
        public string[]? ServiceProductCatalog { get; set; }

        public Companie()
        {
            Id = Guid.NewGuid();
        }

        public Companie(string tradename, string businesssector, string foundingdate)
        {
            Id = Guid.NewGuid();
            BusinessSector = businesssector ?? throw new ArgumentNullException(nameof(businesssector));
            TradeName = tradename ?? throw new ArgumentNullException(nameof(tradename));
            DateTime date; 
            if (DateTime.TryParse(foundingdate, out date))
                FoundingDate =  date;
            else 
                throw new ArgumentException(nameof(foundingdate));
        }
    }