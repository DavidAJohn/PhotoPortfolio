using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Data;

public class PreferencesRepository : BaseRepository<Preferences>, IPreferencesRepository
{
    private readonly MongoContext _context;
    private readonly static string _collectionName = "preferences";

    public PreferencesRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
    }
}
