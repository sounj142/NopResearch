using System.Collections.Generic;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Security;
using Research.Core.Interface.Service;

namespace Research.Services.Security
{
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        // danh sách những quyền trong admin đã được chuyển qua PermissionRecord

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[] 
            {
                PermissionRecord.AccessAdminPanel,
                PermissionRecord.AllowCustomerImpersonation,
                PermissionRecord.ManageProducts,
                PermissionRecord.ManageCategories,
                PermissionRecord.ManageManufacturers,
                PermissionRecord.ManageProductReviews,
                PermissionRecord.ManageProductTags,
                PermissionRecord.ManageAttributes,
                PermissionRecord.ManageCustomers,
                PermissionRecord.ManageVendors,
                PermissionRecord.ManageCurrentCarts,
                PermissionRecord.ManageOrders,
                PermissionRecord.ManageRecurringPayments,
                PermissionRecord.ManageGiftCards,
                PermissionRecord.ManageReturnRequests,
                PermissionRecord.OrderCountryReport,
                PermissionRecord.ManageAffiliates,
                PermissionRecord.ManageCampaigns,
                PermissionRecord.ManageDiscounts,
                PermissionRecord.ManageNewsletterSubscribers,
                PermissionRecord.ManagePolls,
                PermissionRecord.ManageNews,
                PermissionRecord.ManageBlog,
                PermissionRecord.ManageWidgets,
                PermissionRecord.ManageTopics,
                PermissionRecord.ManageForums,
                PermissionRecord.ManageMessageTemplates,
                PermissionRecord.ManageCountries,
                PermissionRecord.ManageLanguages,
                PermissionRecord.ManageSettings,
                PermissionRecord.ManagePaymentMethods,
                PermissionRecord.ManageExternalAuthenticationMethods,
                PermissionRecord.ManageTaxSettings,
                PermissionRecord.ManageShippingSettings,
                PermissionRecord.ManageCurrencies,
                PermissionRecord.ManageMeasures,
                PermissionRecord.ManageActivityLog,
                PermissionRecord.ManageAcl,
                PermissionRecord.ManageEmailAccounts,
                PermissionRecord.ManageStores,
                PermissionRecord.ManagePlugins,
                PermissionRecord.ManageSystemLog,
                PermissionRecord.ManageMessageQueue,
                PermissionRecord.ManageMaintenance,
                PermissionRecord.HtmlEditorManagePictures,
                PermissionRecord.ManageScheduleTasks,
                PermissionRecord.DisplayPrices,
                PermissionRecord.EnableShoppingCart,
                PermissionRecord.EnableWishlist,
                PermissionRecord.PublicStoreAllowNavigation
                // nếu có bổ sung loại quyền con mới, nhớ thêm vào cả khai báo static trong PermissionRecord và cả ở đây
            };
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Administrators,
                    PermissionRecords = new[] 
                    {
                        PermissionRecord.AccessAdminPanel,
                        PermissionRecord.AllowCustomerImpersonation,
                        PermissionRecord.ManageProducts,
                        PermissionRecord.ManageCategories,
                        PermissionRecord.ManageManufacturers,
                        PermissionRecord.ManageProductReviews,
                        PermissionRecord.ManageProductTags,
                        PermissionRecord.ManageAttributes,
                        PermissionRecord.ManageCustomers,
                        PermissionRecord.ManageVendors,
                        PermissionRecord.ManageCurrentCarts,
                        PermissionRecord.ManageOrders,
                        PermissionRecord.ManageRecurringPayments,
                        PermissionRecord.ManageGiftCards,
                        PermissionRecord.ManageReturnRequests,
                        PermissionRecord.OrderCountryReport,
                        PermissionRecord.ManageAffiliates,
                        PermissionRecord.ManageCampaigns,
                        PermissionRecord.ManageDiscounts,
                        PermissionRecord.ManageNewsletterSubscribers,
                        PermissionRecord.ManagePolls,
                        PermissionRecord.ManageNews,
                        PermissionRecord.ManageBlog,
                        PermissionRecord.ManageWidgets,
                        PermissionRecord.ManageTopics,
                        PermissionRecord.ManageForums,
                        PermissionRecord.ManageMessageTemplates,
                        PermissionRecord.ManageCountries,
                        PermissionRecord.ManageLanguages,
                        PermissionRecord.ManageSettings,
                        PermissionRecord.ManagePaymentMethods,
                        PermissionRecord.ManageExternalAuthenticationMethods,
                        PermissionRecord.ManageTaxSettings,
                        PermissionRecord.ManageShippingSettings,
                        PermissionRecord.ManageCurrencies,
                        PermissionRecord.ManageMeasures,
                        PermissionRecord.ManageActivityLog,
                        PermissionRecord.ManageAcl,
                        PermissionRecord.ManageEmailAccounts,
                        PermissionRecord.ManageStores,
                        PermissionRecord.ManagePlugins,
                        PermissionRecord.ManageSystemLog,
                        PermissionRecord.ManageMessageQueue,
                        PermissionRecord.ManageMaintenance,
                        PermissionRecord.HtmlEditorManagePictures,
                        PermissionRecord.ManageScheduleTasks,
                        PermissionRecord.DisplayPrices,
                        PermissionRecord.EnableShoppingCart,
                        PermissionRecord.EnableWishlist,
                        PermissionRecord.PublicStoreAllowNavigation
                    }
                },
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.ForumModerators,
                    PermissionRecords = new []
                    {
                        PermissionRecord.DisplayPrices,
                        PermissionRecord.EnableShoppingCart,
                        PermissionRecord.EnableWishlist,
                        PermissionRecord.PublicStoreAllowNavigation
                    }
                },
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Guests,
                    PermissionRecords = new[] 
                    {
                        PermissionRecord.DisplayPrices,
                        PermissionRecord.EnableShoppingCart,
                        PermissionRecord.EnableWishlist,
                        PermissionRecord.PublicStoreAllowNavigation
                    }
                },
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Registered,
                    PermissionRecords = new[] 
                    {
                        PermissionRecord.DisplayPrices,
                        PermissionRecord.EnableShoppingCart,
                        PermissionRecord.EnableWishlist,
                        PermissionRecord.PublicStoreAllowNavigation
                    }
                },
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Vendors,
                    PermissionRecords = new[] 
                    {
                        PermissionRecord.AccessAdminPanel,
                        PermissionRecord.ManageProducts,
                        PermissionRecord.ManageOrders
                    }
                }
            };
        }
    }
}
