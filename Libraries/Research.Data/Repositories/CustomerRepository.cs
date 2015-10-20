using Research.Core;
using Research.Core.Data;
using Research.Core.Domain.Blogs;
using Research.Core.Domain.Catalog;
using Research.Core.Domain.Common;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Forums;
using Research.Core.Domain.News;
using Research.Core.Domain.Orders;
using Research.Core.Domain.Polls;
using Research.Core.Infrastructure;
using Research.Core.Interface.Data;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Research.Data.Repositories
{
    public partial class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IUnitOfWork unitOfWork)
            :base(unitOfWork)
        {
        }

        public virtual int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, DateTime? lastActivityDateUtcFrom,
            DateTime? lastActivityDateUtcTo, bool onlyWithoutShoppingCart)
        {
            var engine = EngineContext.Current;
            var commonSettings = engine.Resolve<CommonSettings>();
            var dataProvider = engine.Resolve<IDataProvider>();
            int totalRecordsDeleted;

            if (commonSettings.UseStoredProceduresIfSupported && dataProvider.StoredProceduredSupported)
            {
                // nếu database có hỗ trợ store proc thì ưu tiên dùng store proc, cách này nhanh hơn rất nhiều so với cài đặt Linq
                #region Stored proc

                // chuẩn bị các tham số
                var pOnlyWithoutShoppingCart = dataProvider.GetParameter();
                pOnlyWithoutShoppingCart.ParameterName = "OnlyWithoutShoppingCart";
                pOnlyWithoutShoppingCart.Value = onlyWithoutShoppingCart;
                pOnlyWithoutShoppingCart.DbType = DbType.Boolean;

                var pCreatedFromUtc = dataProvider.GetParameter();
                pCreatedFromUtc.ParameterName = "CreatedFromUtc";
                pCreatedFromUtc.Value = createdFromUtc.HasValue ? (object)createdFromUtc.Value : DBNull.Value;
                pCreatedFromUtc.DbType = DbType.DateTime;

                var pCreatedToUtc = dataProvider.GetParameter();
                pCreatedToUtc.ParameterName = "CreatedToUtc";
                pCreatedToUtc.Value = createdToUtc.HasValue ? (object)createdToUtc.Value : DBNull.Value;
                pCreatedToUtc.DbType = DbType.DateTime;

                var pLastActivityDateUtcFrom = dataProvider.GetParameter();
                pLastActivityDateUtcFrom.ParameterName = "LastActivityDateUtcFrom";
                pLastActivityDateUtcFrom.Value = lastActivityDateUtcFrom.HasValue ? (object)lastActivityDateUtcFrom.Value : DBNull.Value;
                pLastActivityDateUtcFrom.DbType = DbType.DateTime;

                var pLastActivityDateUtcTo = dataProvider.GetParameter();
                pLastActivityDateUtcTo.ParameterName = "LastActivityDateUtcTo";
                pLastActivityDateUtcTo.Value = lastActivityDateUtcTo.HasValue ? (object)lastActivityDateUtcTo.Value : DBNull.Value;
                pLastActivityDateUtcTo.DbType = DbType.DateTime;

                var pTotalRecordsDeleted = dataProvider.GetParameter();
                pTotalRecordsDeleted.ParameterName = "TotalRecordsDeleted";
                pTotalRecordsDeleted.Direction = ParameterDirection.Output;
                pTotalRecordsDeleted.DbType = DbType.Int32;

                // gọi stored proc
                _context.ExecuteSqlCommand(
                    "exec [DeleteGuestsV2] @OnlyWithoutShoppingCart, @CreatedFromUtc, @CreatedToUtc, @LastActivityDateUtcFrom, @LastActivityDateUtcTo, @TotalRecordsDeleted output",
                    false, null, pOnlyWithoutShoppingCart, pCreatedFromUtc, pCreatedToUtc, 
                    pLastActivityDateUtcFrom, pLastActivityDateUtcTo, pTotalRecordsDeleted);

                totalRecordsDeleted = (pTotalRecordsDeleted.Value != DBNull.Value ? Convert.ToInt32(pTotalRecordsDeleted.Value) : 0);
                #endregion
            }
            else
            {
                // database ko hỗ trợ, dùng linq và transaction
                #region Stored proc

                var customerRoleRepository = engine.Resolve<IRepository<CustomerRole>>();
                var orderRepository = engine.Resolve<IRepository<Order>>();
                var forumPostRepository = engine.Resolve<IRepository<ForumPost>>();
                var forumTopicRepository = engine.Resolve<IRepository<ForumTopic>>();
                var blogCommentRepository = engine.Resolve<IRepository<BlogComment>>();
                var newsCommentRepository = engine.Resolve<IRepository<NewsComment>>();
                var pollVotingRecordRepository = engine.Resolve<IRepository<PollVotingRecord>>();
                var productReviewRepository = engine.Resolve<IRepository<ProductReview>>();
                var productReviewHelpfulnessRepository = engine.Resolve<IRepository<ProductReviewHelpfulness>>();
                var gaRepository = engine.Resolve<IRepository<GenericAttribute>>();


                var guestRole = customerRoleRepository.Table
                    .Where(p => p.SystemName == SystemCustomerRoleNames.Guests)
                    .FirstOrDefault();
                if (guestRole == null) throw new ResearchException("Không tìm thấy vai trò Guest");

                var query = this.Table;
                if (createdFromUtc.HasValue) query = query.Where(c => c.CreatedOnUtc >= createdFromUtc.Value);
                if (createdToUtc.HasValue) query = query.Where(c => c.CreatedOnUtc <= createdToUtc.Value);

                if (lastActivityDateUtcFrom.HasValue) query = query.Where(c => c.LastActivityDateUtc >= lastActivityDateUtcFrom.Value);
                if (lastActivityDateUtcTo.HasValue) query = query.Where(c => c.LastActivityDateUtc <= lastActivityDateUtcTo.Value);

                query = query.Where(c => c.CustomerRoles.Any(r => r.Id == guestRole.Id));

                // ngăn ko cho xóa các tài khoản thuộc loại SystemAccount
                // nếu yêu cầu là ko đc xóa nếu customer đang có gì đó trong giỏ hàng/wish lish
                if (onlyWithoutShoppingCart) query = query.Where(c => !c.IsSystemAccount && !c.ShoppingCartItems.Any());

                // để có thể xóa thì phải đảm bảo là guest này thực sự là "rác", ko có đơn hàng, ko có comment, ko có reviews ...
                // no orders
                query = from c in query
                        join o in orderRepository.Table on c.Id equals o.CustomerId into c_o
                        from o in c_o.DefaultIfEmpty()
                        where !c_o.Any()
                        select c;
                //no blog comments
                query = from c in query
                        join bc in blogCommentRepository.Table on c.Id equals bc.CustomerId into c_bc
                        from bc in c_bc.DefaultIfEmpty()
                        where !c_bc.Any()
                        select c;
                //no news comments
                query = from c in query
                        join nc in newsCommentRepository.Table on c.Id equals nc.CustomerId into c_nc
                        from nc in c_nc.DefaultIfEmpty()
                        where !c_nc.Any()
                        select c;
                //no product reviews
                query = from c in query
                        join pr in productReviewRepository.Table on c.Id equals pr.CustomerId into c_pr
                        from pr in c_pr.DefaultIfEmpty()
                        where !c_pr.Any()
                        select c;
                //no product reviews helpfulness
                query = from c in query
                        join prh in productReviewHelpfulnessRepository.Table on c.Id equals prh.CustomerId into c_prh
                        from prh in c_prh.DefaultIfEmpty()
                        where !c_prh.Any()
                        select c;
                //no poll voting
                query = from c in query
                        join pvr in pollVotingRecordRepository.Table on c.Id equals pvr.CustomerId into c_pvr
                        from pvr in c_pvr.DefaultIfEmpty()
                        where !c_pvr.Any()
                        select c;
                //no forum posts 
                query = from c in query
                        join fp in forumPostRepository.Table on c.Id equals fp.CustomerId into c_fp
                        from fp in c_fp.DefaultIfEmpty()
                        where !c_fp.Any()
                        select c;
                //no forum topics
                query = from c in query
                        join ft in forumTopicRepository.Table on c.Id equals ft.CustomerId into c_ft
                        from ft in c_ft.DefaultIfEmpty()
                        where !c_ft.Any()
                        select c;

                // lọc lại để chỉ lấy các customer distinct
                query = from c in query
                        group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                var needToDeleteCustomers = query.ToList();

                if (needToDeleteCustomers.Count > 0)
                {
                    foreach (var customer in needToDeleteCustomers)
                    {
                        // delete generic attribute
                        var attributes = gaRepository.Table
                            .Where(p => p.EntityId == customer.Id && p.KeyGroup == "Customer")
                            .ToList();
                        gaRepository.Delete(attributes);

                        // delete customer
                        this.Delete(customer);
                    }

                    totalRecordsDeleted = (_unitOfWork.SaveChanges() > 0 ? needToDeleteCustomers.Count : 0);
                }
                else totalRecordsDeleted = 0;
                

                #endregion
            }

            return totalRecordsDeleted;
        }
    }
}
