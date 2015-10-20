using Research.Core.Domain.Localization;
using Research.Core.Interface.Data;

namespace Research.Data.Repositories
{
    public partial class LocalizationRepository : EfRepository<LocaleStringResource>, ILocalizationRepository
    {
        public LocalizationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }
    }
}
