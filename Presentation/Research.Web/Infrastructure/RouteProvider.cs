using System.Web.Mvc;
using System.Web.Routing;
using Research.Web.Framework.Localization;
using Research.Web.Framework.Mvc.Routes;

namespace Research.Web.Infrastructure
{
    /// <summary>
    /// Lớp cài đặt IRouteProvider sẽ được thực thi đầu tiên ( Priority = 0), chứa các route thường gặp nhất, và các route 1 phân đoạn
    /// mà có khả năng xung đột với Generic route sẽ được đăng ký sau đó
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            // đăng ký các route chính yếu nhất của website. Ghi nhớ là hệ thống route này ko cho phép ta có thể đa ngôn ngữ url dạng
            // dang-ky;register mà chỉ dùng được duy nhất 1 url là register. Đa ngôn ngữ chỉ có thể dùng với cơ chế url của 
            // GenericRoute, chỉ 1 phân đoạn, khá hạn chế



            // Đẩy KeepAlive lên đầu vì đây là url cần được xử lý nhanh nhất, thậm chí đối với Url này, chúng ta cần tránh tất cả mọi công đoạn có thể
            // làm suy giảm hiệu năng như là tìm store hiện hành, tìm language hiện hành, ..v.v.v.
            // Tất cả những gì mà KeepAlive chỉ là theo định kỳ gọi đến để duy trì Session, chỉ có thế
            // link cho KeepAlive sẽ ko có phân đoạn ngôn ngữ, vì ko cần phải có nó
            // sẽ chỉnh sửa thêm ....
            routes.MapRoute("KeepAlive",
                            "keepalive/index",
                            new { controller = "KeepAlive", action = "Index" },
                            new[] { "Research.Web.Controllers" });

            //home page
            routes.MapLocalizedRoute("HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new[] { "Research.Web.Controllers" });

            // route này ko dùng LocalizedRoute, đồng nghĩa với việc nó sẽ ko thể hỗ trợ phân đoạn ngôn ngữ trong url
            //widgets
            //we have this route for performance optimization because named routes are MUCH faster than usual Html.Action(...)
            //and this route is highly used
            routes.MapRoute("WidgetsByZone",
                            "widgetsbyzone/",
                            new { controller = "Widget", action = "WidgetsByZone" },
                            new[] { "Research.Web.Controllers" });

            //login
            // sử dụng LocalizedRoute chỉ là để cho phép url có thể có thêm các phân đoạn ngôn ngữ mà vẫn được 
            // phân giải link tới 1 cách bình thường, và được add thêm phân đoạn ngôn ngữ với link sinh trong view, chứ chả có gì đặc biệt
            routes.MapLocalizedRoute("Login",
                            "login/",
                            new { controller = "Customer", action = "Login" },
                            new[] { "Research.Web.Controllers" });
            //register
            routes.MapLocalizedRoute("Register",
                            "register/",
                            new { controller = "Customer", action = "Register" },
                            new[] { "Research.Web.Controllers" });
            //logout
            routes.MapLocalizedRoute("Logout",
                            "logout/",
                            new { controller = "Customer", action = "Logout" },
                            new[] { "Research.Web.Controllers" });

            //shopping cart
            routes.MapLocalizedRoute("ShoppingCart",
                            "cart/",
                            new { controller = "ShoppingCart", action = "Cart" },
                            new[] { "Research.Web.Controllers" });
            //wishlist
            routes.MapLocalizedRoute("Wishlist",
                            "wishlist/{customerGuid}",
                            new { controller = "ShoppingCart", action = "Wishlist", customerGuid = UrlParameter.Optional },
                            new[] { "Research.Web.Controllers" });

            //customer account links
            routes.MapLocalizedRoute("CustomerInfo",
                            "customer/info",
                            new { controller = "Customer", action = "Info" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddresses",
                            "customer/addresses",
                            new { controller = "Customer", action = "Addresses" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerOrders",
                            "order/history",
                            new { controller = "Order", action = "CustomerOrders" },
                            new[] { "Research.Web.Controllers" });

            //contact us
            routes.MapLocalizedRoute("ContactUs",
                            "contactus",
                            new { controller = "Common", action = "ContactUs" },
                            new[] { "Research.Web.Controllers" });
            //sitemap
            routes.MapLocalizedRoute("Sitemap",
                            "sitemap",
                            new { controller = "Common", action = "Sitemap" },
                            new[] { "Research.Web.Controllers" });

            //product search
            routes.MapLocalizedRoute("ProductSearch",
                            "search/",
                            new { controller = "Catalog", action = "Search" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ProductSearchAutoComplete",
                            "catalog/searchtermautocomplete",
                            new { controller = "Catalog", action = "SearchTermAutoComplete" },
                            new[] { "Research.Web.Controllers" });

            //change currency (AJAX link)
            routes.MapLocalizedRoute("ChangeCurrency",
                            "changecurrency/{customercurrency}",
                            new { controller = "Common", action = "SetCurrency" },
                            new { customercurrency = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //change language (AJAX link)
            routes.MapLocalizedRoute("ChangeLanguage",
                            "changelanguage/{langid}",
                            new { controller = "Common", action = "SetLanguage" },
                            new { langid = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //change tax (AJAX link)
            routes.MapLocalizedRoute("ChangeTaxType",
                            "changetaxtype/{customertaxtype}",
                            new { controller = "Common", action = "SetTaxType" },
                            new { customertaxtype = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //recently viewed products
            routes.MapLocalizedRoute("RecentlyViewedProducts",
                            "recentlyviewedproducts/",
                            new { controller = "Product", action = "RecentlyViewedProducts" },
                            new[] { "Research.Web.Controllers" });
            //recently added products
            routes.MapLocalizedRoute("RecentlyAddedProducts",
                            "newproducts/",
                            new { controller = "Product", action = "RecentlyAddedProducts" },
                            new[] { "Research.Web.Controllers" });
            //blog
            routes.MapLocalizedRoute("Blog",
                            "blog",
                            new { controller = "Blog", action = "List" },
                            new[] { "Research.Web.Controllers" });
            //news
            routes.MapLocalizedRoute("NewsArchive",
                            "news",
                            new { controller = "News", action = "List" },
                            new[] { "Research.Web.Controllers" });

            //forum
            routes.MapLocalizedRoute("Boards",
                            "boards",
                            new { controller = "Boards", action = "Index" },
                            new[] { "Research.Web.Controllers" });

            //compare products
            routes.MapLocalizedRoute("CompareProducts",
                            "compareproducts/",
                            new { controller = "Product", action = "CompareProducts" },
                            new[] { "Research.Web.Controllers" });

            //product tags
            routes.MapLocalizedRoute("ProductTagsAll",
                            "producttag/all/",
                            new { controller = "Catalog", action = "ProductTagsAll" },
                            new[] { "Research.Web.Controllers" });

            //manufacturers
            routes.MapLocalizedRoute("ManufacturerList",
                            "manufacturer/all/",
                            new { controller = "Catalog", action = "ManufacturerAll" },
                            new[] { "Research.Web.Controllers" });
            //vendors
            routes.MapLocalizedRoute("VendorList",
                            "vendor/all/",
                            new { controller = "Catalog", action = "VendorAll" },
                            new[] { "Research.Web.Controllers" });

            //add product to cart (without any attributes and options). used on catalog pages.
            routes.MapLocalizedRoute("AddProductToCart-Catalog",
                            "addproducttocart/catalog/{productId}/{shoppingCartTypeId}/{quantity}",
                            new { controller = "ShoppingCart", action = "AddProductToCart_Catalog" },
                            new { productId = @"\d+", shoppingCartTypeId = @"\d+", quantity = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //add product to cart (with attributes and options). used on the product details pages.
            routes.MapLocalizedRoute("AddProductToCart-Details",
                            "addproducttocart/details/{productId}/{shoppingCartTypeId}",
                            new { controller = "ShoppingCart", action = "AddProductToCart_Details" },
                            new { productId = @"\d+", shoppingCartTypeId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //product tags
            routes.MapLocalizedRoute("ProductsByTag",
                            "producttag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag", SeName = UrlParameter.Optional },
                            new { productTagId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //comparing products
            routes.MapLocalizedRoute("AddProductToCompare",
                            "compareproducts/add/{productId}",
                            new { controller = "Product", action = "AddProductToCompareList" },
                            new { productId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //product email a friend
            routes.MapLocalizedRoute("ProductEmailAFriend",
                            "productemailafriend/{productId}",
                            new { controller = "Product", action = "ProductEmailAFriend" },
                            new { productId = @"\d+" },
                            new[] { "Research.Web.Controllers" });


            //reviews
            routes.MapLocalizedRoute("ProductReviews",
                            "productreviews/{productId}",
                            new { controller = "Product", action = "ProductReviews" },
                            new[] { "Research.Web.Controllers" });
            //back in stock notifications
            routes.MapLocalizedRoute("BackInStockSubscribePopup",
                            "backinstocksubscribe/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribePopup" },
                            new { productId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //downloads
            routes.MapRoute("GetSampleDownload",
                            "download/sample/{productid}",
                            new { controller = "Download", action = "Sample" },
                            new { productid = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //checkout pages
            routes.MapLocalizedRoute("Checkout",
                            "checkout/",
                            new { controller = "Checkout", action = "Index" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingAddress",
                            "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutSelectShippingAddress",
                            "checkout/selectshippingaddress",
                            new { controller = "Checkout", action = "SelectShippingAddress" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutBillingAddress",
                            "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutSelectBillingAddress",
                            "checkout/selectbillingaddress",
                            new { controller = "Checkout", action = "SelectBillingAddress" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentInfo",
                            "checkout/paymentinfo",
                            new { controller = "Checkout", action = "PaymentInfo" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutCompleted",
                            "checkout/completed/{orderId}",
                            new { controller = "Checkout", action = "Completed", orderId = UrlParameter.Optional },
                            new { orderId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //subscribe newsletters
            routes.MapLocalizedRoute("SubscribeNewsletter",
                            "subscribenewsletter",
                            new { controller = "Newsletter", action = "SubscribeNewsletter" },
                            new[] { "Research.Web.Controllers" });

            //email wishlist
            routes.MapLocalizedRoute("EmailWishlist",
                            "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" },
                            new[] { "Research.Web.Controllers" });

            //login page for checkout as guest
            routes.MapLocalizedRoute("LoginCheckoutAsGuest",
                            "login/checkoutasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true },
                            new[] { "Research.Web.Controllers" });
            //register result page
            routes.MapLocalizedRoute("RegisterResult",
                            "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" },
                            new { resultId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //check username availability
            routes.MapLocalizedRoute("CheckUsernameAvailability",
                            "customer/checkusernameavailability",
                            new { controller = "Customer", action = "CheckUsernameAvailability" },
                            new[] { "Research.Web.Controllers" });

            //passwordrecovery
            routes.MapLocalizedRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" },
                            new[] { "Research.Web.Controllers" });
            //password recovery confirmation
            routes.MapLocalizedRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" },
                            new[] { "Research.Web.Controllers" });

            //topics
            routes.MapLocalizedRoute("TopicPopup",
                            "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" },
                            new[] { "Research.Web.Controllers" });

            //blog
            routes.MapLocalizedRoute("BlogByTag",
                            "blog/tag/{tag}",
                            new { controller = "Blog", action = "BlogByTag" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("BlogByMonth",
                            "blog/month/{month}",
                            new { controller = "Blog", action = "BlogByMonth" },
                            new[] { "Research.Web.Controllers" });
            //blog RSS
            routes.MapLocalizedRoute("BlogRSS",
                            "blog/rss/{languageId}",
                            new { controller = "Blog", action = "ListRss" },
                            new { languageId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //news RSS
            routes.MapLocalizedRoute("NewsRSS",
                            "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" },
                            new { languageId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //set review helpfulness (AJAX link)
            routes.MapRoute("SetProductReviewHelpfulness",
                            "setproductreviewhelpfulness",
                            new { controller = "Product", action = "SetProductReviewHelpfulness" },
                            new[] { "Research.Web.Controllers" });

            //customer account links
            routes.MapLocalizedRoute("CustomerReturnRequests",
                            "returnrequest/history",
                            new { controller = "ReturnRequest", action = "CustomerReturnRequests" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerDownloadableProducts",
                            "customer/downloadableproducts",
                            new { controller = "Customer", action = "DownloadableProducts" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerBackInStockSubscriptions",
                            "backinstocksubscriptions/manage",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerBackInStockSubscriptionsPaged",
                            "backinstocksubscriptions/manage/{page}",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions", page = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerRewardPoints",
                            "rewardpoints/history",
                            new { controller = "Order", action = "CustomerRewardPoints" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerChangePassword",
                            "customer/changepassword",
                            new { controller = "Customer", action = "ChangePassword" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAvatar",
                            "customer/avatar",
                            new { controller = "Customer", action = "Avatar" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("AccountActivation",
                            "customer/activation",
                            new { controller = "Customer", action = "AccountActivation" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerForumSubscriptions",
                            "boards/forumsubscriptions",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerForumSubscriptionsPaged",
                            "boards/forumsubscriptions/{page}",
                            new { controller = "Boards", action = "CustomerForumSubscriptions", page = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddressDelete",
                            "customer/addressdelete/{addressId}",
                            new { controller = "Customer", action = "AddressDelete" },
                            new { addressId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddressEdit",
                            "customer/addressedit/{addressId}",
                            new { controller = "Customer", action = "AddressEdit" },
                            new { addressId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddressAdd",
                            "customer/addressadd",
                            new { controller = "Customer", action = "AddressAdd" },
                            new[] { "Research.Web.Controllers" });

            //customer profile page
            routes.MapLocalizedRoute("CustomerProfile",
                            "profile/{id}",
                            new { controller = "Profile", action = "Index" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerProfilePaged",
                            "profile/{id}/page/{page}",
                            new { controller = "Profile", action = "Index" },
                            new { id = @"\d+", page = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //orders
            routes.MapLocalizedRoute("OrderDetails",
                            "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" },
                            new { orderId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ShipmentDetails",
                            "orderdetails/shipment/{shipmentId}",
                            new { controller = "Order", action = "ShipmentDetails" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ReturnRequest",
                            "returnrequest/{orderId}",
                            new { controller = "ReturnRequest", action = "ReturnRequest" },
                            new { orderId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ReOrder",
                            "reorder/{orderId}",
                            new { controller = "Order", action = "ReOrder" },
                            new { orderId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("GetOrderPdfInvoice",
                            "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PrintOrderDetails",
                            "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" },
                            new[] { "Research.Web.Controllers" });

            //order downloads
            routes.MapRoute("GetDownload",
                            "download/getdownload/{orderItemId}/{agree}",
                            new { controller = "Download", action = "GetDownload", agree = UrlParameter.Optional },
                            new { orderItemId = new GuidConstraint(false) }, // orderItemId phải là chuỗi có dạng Guid, ko chấp nhận Guid.Empty
                            new[] { "Research.Web.Controllers" });
            routes.MapRoute("GetLicense",
                            "download/getlicense/{orderItemId}/",
                            new { controller = "Download", action = "GetLicense" },
                            new { orderItemId = new GuidConstraint(false) },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("DownloadUserAgreement",
                            "customer/useragreement/{orderItemId}",
                            new { controller = "Customer", action = "UserAgreement" },
                            new { orderItemId = new GuidConstraint(false) },
                            new[] { "Research.Web.Controllers" });
            routes.MapRoute("GetOrderNoteFile",
                            "download/ordernotefile/{ordernoteid}",
                            new { controller = "Download", action = "GetOrderNoteFile" },
                            new { ordernoteid = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //poll vote AJAX link
            routes.MapLocalizedRoute("PollVote",
                            "poll/vote",
                            new { controller = "Poll", action = "Vote" },
                            new[] { "Research.Web.Controllers" });

            //comparing products
            routes.MapLocalizedRoute("RemoveProductFromCompareList",
                            "compareproducts/remove/{productId}",
                            new { controller = "Product", action = "RemoveProductFromCompareList" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ClearCompareList",
                            "clearcomparelist/",
                            new { controller = "Product", action = "ClearCompareList" },
                            new[] { "Research.Web.Controllers" });

            //recently added products RSS
            routes.MapLocalizedRoute("RecentlyAddedProductsRSS",
                            "newproducts/rss",
                            new { controller = "Product", action = "RecentlyAddedProductsRss" },
                            new[] { "Research.Web.Controllers" });

            //get state list by country ID  (AJAX link)
            routes.MapRoute("GetStatesByCountryId",
                            "country/getstatesbycountryid/",
                            new { controller = "Country", action = "GetStatesByCountryId" },
                            new[] { "Research.Web.Controllers" });

            //EU Cookie law accept button handler (AJAX link)
            routes.MapRoute("EuCookieLawAccept",
                            "eucookielawaccept",
                            new { controller = "Common", action = "EuCookieLawAccept" },
                            new[] { "Research.Web.Controllers" });

            //authenticate topic AJAX link
            routes.MapLocalizedRoute("TopicAuthenticate",
                            "topic/authenticate",
                            new { controller = "Topic", action = "Authenticate" },
                            new[] { "Research.Web.Controllers" });

            //product attributes with "upload file" type
            routes.MapLocalizedRoute("UploadFileProductAttribute",
                            "uploadfileproductattribute/{attributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileProductAttribute" },
                            new { attributeId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            //checkout attributes with "upload file" type
            routes.MapLocalizedRoute("UploadFileCheckoutAttribute",
                            "uploadfilecheckoutattribute/{attributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileCheckoutAttribute" },
                            new { attributeId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //forums
            routes.MapLocalizedRoute("ActiveDiscussions",
                            "boards/activediscussions",
                            new { controller = "Boards", action = "ActiveDiscussions" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ActiveDiscussionsRSS",
                            "boards/activediscussionsrss",
                            new { controller = "Boards", action = "ActiveDiscussionsRSS" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PostEdit",
                            "boards/postedit/{id}",
                            new { controller = "Boards", action = "PostEdit" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PostDelete",
                            "boards/postdelete/{id}",
                            new { controller = "Boards", action = "PostDelete" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PostCreate",
                            "boards/postcreate/{id}",
                            new { controller = "Boards", action = "PostCreate" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PostCreateQuote",
                            "boards/postcreate/{id}/{quote}",
                            new { controller = "Boards", action = "PostCreate" },
                            new { id = @"\d+", quote = @"\d+" },
                            new[] { "Research.Web.Controllers" });


            routes.MapLocalizedRoute("TopicEdit",
                            "boards/topicedit/{id}",
                            new { controller = "Boards", action = "TopicEdit" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicDelete",
                            "boards/topicdelete/{id}",
                            new { controller = "Boards", action = "TopicDelete" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicCreate",
                            "boards/topiccreate/{id}",
                            new { controller = "Boards", action = "TopicCreate" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicMove",
                            "boards/topicmove/{id}",
                            new { controller = "Boards", action = "TopicMove" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicWatch",
                            "boards/topicwatch/{id}",
                            new { controller = "Boards", action = "TopicWatch" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicSlug",
                            "boards/topic/{id}/{slug}",
                            new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("TopicSlugPaged",
                            "boards/topic/{id}/{slug}/page/{page}",
                            new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional, page = UrlParameter.Optional },
                            new { id = @"\d+", page = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ForumWatch",
                            "boards/forumwatch/{id}",
                            new { controller = "Boards", action = "ForumWatch" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });


            routes.MapLocalizedRoute("ForumRSS",
                            "boards/forumrss/{id}",
                            new { controller = "Boards", action = "ForumRSS" },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ForumSlug",
                            "boards/forum/{id}/{slug}",
                            new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ForumSlugPaged",
                            "boards/forum/{id}/{slug}/page/{page}",
                            new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional, page = UrlParameter.Optional },
                            new { id = @"\d+", page = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ForumGroupSlug",
                            "boards/forumgroup/{id}/{slug}",
                            new { controller = "Boards", action = "ForumGroup", slug = UrlParameter.Optional },
                            new { id = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("Search",
                            "boards/search",
                            new { controller = "Boards", action = "Search" },
                            new[] { "Research.Web.Controllers" });

            //private messages
            routes.MapLocalizedRoute("PrivateMessages",
                            "privatemessages/{tab}",
                            new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PrivateMessagesPaged",
                            "privatemessages/{tab}/page/{page}",
                            new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "Research.Web.Controllers" });



            routes.MapLocalizedRoute("PrivateMessagesInbox",
                            "inboxupdate",
                            new { controller = "PrivateMessages", action = "InboxUpdate" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("PrivateMessagesSent",
                            "sentupdate",
                            new { controller = "PrivateMessages", action = "SentUpdate" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("SendPM",
                            "sendpm/{toCustomerId}",
                            new { controller = "PrivateMessages", action = "SendPM" },
                            new { toCustomerId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("SendPMReply",
                            "sendpm/{toCustomerId}/{replyToMessageId}",
                            new { controller = "PrivateMessages", action = "SendPM" },
                            new { toCustomerId = @"\d+", replyToMessageId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("ViewPM",
                            "viewpm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "ViewPM" },
                            new { privateMessageId = @"\d+" },
                            new[] { "Research.Web.Controllers" });
            routes.MapLocalizedRoute("DeletePM",
                            "deletepm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "DeletePM" },
                            new { privateMessageId = @"\d+" },
                            new[] { "Research.Web.Controllers" });

            //activate newsletters
            routes.MapLocalizedRoute("NewsletterActivation",
                            "newsletter/subscriptionactivation/{token}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" },
                            new { token = new GuidConstraint(false) },
                            new[] { "Research.Web.Controllers" });



            //robots.txt
            routes.MapRoute("robots.txt",
                            "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" },
                            new[] { "Research.Web.Controllers" });

            //sitemap (XML)
            routes.MapLocalizedRoute("sitemap.xml",
                            "sitemap.xml",
                            new { controller = "Common", action = "SitemapXml" },
                            new[] { "Research.Web.Controllers" });

            //store closed
            routes.MapLocalizedRoute("StoreClosed",
                            "storeclosed",
                            new { controller = "Common", action = "StoreClosed" },
                            new[] { "Research.Web.Controllers" });

            //install
            routes.MapRoute("Installation",
                            "install",
                            new { controller = "Install", action = "Index" },
                            new[] { "Research.Web.Controllers" });

            //page not found
            routes.MapLocalizedRoute("PageNotFound",
                            "page-not-found",
                            new { controller = "Common", action = "PageNotFound" },
                            new[] { "Research.Web.Controllers" });
        }

        public int Priority
        {
            // nói chung 0 là số cao nhất rồi, mấy cái kia toàn là số âm cả
            get { return 0; }
        }
    }
}