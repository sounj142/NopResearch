using Research.Core.Interface.Service;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Customers.ViewModels;
using System;
using System.Linq;
using Research.Core;

namespace Research.Services.Customers
{
    // đây là 1 service ở mức cao, sử dụng lại các service khác chứ ko hề dùng đến Repossitory
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields, Ctors, Properties

        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;

        public CustomerRegistrationService(ICustomerService customerService,
            IEncryptionService encryptionService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService,
            IStoreService storeService,
            RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings)
        {
            _customerService = customerService;
            _encryptionService = encryptionService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationService = localizationService;
            _storeService = storeService;
            _rewardPointsSettings = rewardPointsSettings;
            _customerSettings = customerSettings;
        }

        #endregion

        #region Helper

        protected virtual void SetPasswordValue(Customer customer, string password)
        {
            switch (customer.PasswordFormat)
            {
                case PasswordFormat.Hashed:
                    customer.PasswordSalt = _encryptionService.CreateSaltKey(5);
                    customer.Password = _encryptionService.CreatePasswordHash(password, customer.PasswordSalt,
                        _customerSettings.HashedPasswordFormat);
                    break;
                case PasswordFormat.Encrypted:
                    customer.Password = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Clear:
                    customer.Password = password;
                    break;
            }
        }

        #endregion

        #region Methods

        public virtual CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password)
        {
            Customer customer = _customerSettings.UsernamesEnabled ?
                _customerService.GetCustomerByUsername(usernameOrEmail)
                : _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null) return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted) return CustomerLoginResults.Deleted;
            if (!customer.Active) return CustomerLoginResults.NotActive;
            // chỉ cho phép login nếu tài khoản người dùng thuộc role Registered
            if (!customer.IsRegistered()) return CustomerLoginResults.NotRegistered;

            string pwd;
            switch(customer.PasswordFormat)
            {
                case PasswordFormat.Hashed:
                    pwd = _encryptionService.CreatePasswordHash(password, customer.PasswordSalt,
                        _customerSettings.HashedPasswordFormat);
                    break;
                case PasswordFormat.Encrypted:
                    pwd = _encryptionService.EncryptText(password);
                    break;
                default:
                    pwd = password;
                    break;
            }
            if (!string.Equals(pwd, customer.Password, StringComparison.InvariantCulture)) return CustomerLoginResults.WrongPassword;

            // ghi nhận lần login gần đây nhất
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer); // clear cache là ko cần thiết lắm, nhưng để đảm bảo tính đúng đắn cao nhất thì nên clear cache
            return CustomerLoginResults.Successful;
        }

        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.Customer == null) throw new ArgumentException("request.Customer must not null");

            var result = new CustomerRegistrationResult();
            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.SearchEngineCantRegister", false));
                return result;
            }  
            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.BackgroundTaskCantRegister", false));
                return result;
            }
            if (request.Customer.IsRegistered())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.AlreadyRegistered", false));
                return result;
            }
            
            if(string.IsNullOrWhiteSpace(request.Email))
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided", false));
            else if (!CommonHelper.IsValidEmail(request.Email))
                result.AddError(_localizationService.GetResource("Common.WrongEmail", false));

            if (string.IsNullOrWhiteSpace(request.Password))
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided", false));

            if(_customerSettings.UsernamesEnabled && string.IsNullOrWhiteSpace(request.Username))
                result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided", false));
            if (!result.Success) return result;


            request.Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username.Trim();
            request.Email = request.Email.Trim();


            // kiểm tra trùng tài khoản, mặc định coi tài khoản request.Customer là guest, ko hề có email, username, nên phép kiểm tra này
            // ko hề kiểm tra việc tài khoản lấy đc trùng với request.Customer
            if(_customerService.GetCustomerByEmail(request.Email) != null)
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists", false));
            // khác: luôn đảm bảo là user name duy nhất, như thế sẽ có lợi hơn trong xử lý về sau, nhất là các hệ thống chạy email
            // 1 thời gian dài muốn chuyển qua user name
            if (request.Username != null && _customerService.GetCustomerByUsername(request.Username) != null)
                result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists", false));
            if (!result.Success) return result;
                
            
            // ok, chuyển request.Customer thành register user. UnActive/Deleted đều ok hết ???
            var customer = request.Customer;
            customer.Username = request.Username;
            customer.Email = request.Email;
            customer.PasswordFormat = request.PasswordFormat;

            SetPasswordValue(customer, request.Password);
            customer.Active = request.IsApproved; // trạng thái kích hoạt ?

            // loại bỏ vai trò Guest
            var guestRole = customer.CustomerRoles.FirstOrDefault(p =>
                SystemCustomerRoleNames.Guests.Equals(p.SystemName, StringComparison.InvariantCulture));
            if (guestRole != null) customer.CustomerRoles.Remove(guestRole);

            // add customer vào vai trò registered
            var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredRole == null) throw new ResearchException("'Registered' role could not be loaded");
            customer.CustomerRoles.Add(registeredRole);

            // add điểm thưởng cho registed customer nếu chức năng này đc bật
            //if(_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
                //customer.AddRewardPointsHistoryEntry()

            // TODO: thiếu 1 đoạn

            _customerService.UpdateCustomer(customer);
            return result;
        }

        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var result = new PasswordChangeResult();

            if(string.IsNullOrWhiteSpace(request.Email))
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailIsNotProvided", false));
            if (request.ValidateRequest)
            {
                if (string.IsNullOrEmpty(request.OldPassword))
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordIsNotProvided", false));
            }
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided", false));
            if (!result.Success) return result;

            // lấy ra customer theo email, khác: ko cho đổi pass nếu kiểm tra thấy customer là deleted hoặc unactive
            var customer = _customerService.GetCustomerByEmail(request.Email);
            if (customer == null || (request.ValidateRequest && customer.Deleted))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailNotFound", false));
                return result;
            }



            if (request.ValidateRequest) 
            {
                // kiểm tra old password phải đúng 
                string oldPassword = null;
                switch (customer.PasswordFormat)
                {
                    case PasswordFormat.Hashed:
                        oldPassword = _encryptionService.CreatePasswordHash(request.OldPassword, customer.PasswordSalt,
                            _customerSettings.HashedPasswordFormat);
                        break;
                    case PasswordFormat.Encrypted:
                        oldPassword = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Clear:
                        oldPassword = request.OldPassword;
                        break;
                }
                if (!string.Equals(oldPassword, customer.Password, StringComparison.InvariantCulture))
                {
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordDoesntMatch", false));
                    return result;
                }

                if (!customer.Active)
                {
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.IsNotActive", false));
                    return result;
                }
            }
            
            // ok, đến đây thì mọi kiểm tra đều pass, tiến hành đổi password
            customer.PasswordFormat = request.NewPasswordFormat;
            SetPasswordValue(customer, request.NewPassword);

            _customerService.UpdateCustomer(customer);
            return result;
        }

        public virtual void SetEmail(Customer customer, string newEmail)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            if (string.IsNullOrWhiteSpace(newEmail)) throw new ArgumentNullException("newEmail");

            newEmail = newEmail.Trim();
            if (newEmail.Length > _customerSettings.EmailMaxLength)
                throw new ResearchException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailTooLong", false));
            if(!CommonHelper.IsValidEmail(newEmail))
                throw new ResearchException(_localizationService.GetResource("Account.EmailUsernameErrors.NewEmailIsNotValid", false));

            var another = _customerService.GetCustomerByEmail(newEmail);
            if (another != null && another.Id != customer.Id)
                throw new ResearchException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailAlreadyExists", false));

            if (newEmail.Equals(customer.Email, StringComparison.InvariantCulture))
                return;

            string oldEmail = customer.Email;
            customer.Email = newEmail;
            _customerService.UpdateCustomer(customer);

            //update newsletter subscription (if required)
            // điều kiện để update là email cũ khác null, và email mới khác email cũ
            if(!string.IsNullOrEmpty(oldEmail) && !newEmail.Equals(oldEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach(var store in _storeService.GetAllStores())
                {
                    var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(oldEmail, store.Id);
                    if(subscriptionOld != null)
                    {
                        subscriptionOld.Email = newEmail;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                        // xem lại vấn đề publish event ở chỗ này và publish event cho cútomer
                    }
                }
            }
        }

        public virtual void SetUsername(Customer customer, string newUsername)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            if (string.IsNullOrWhiteSpace(newUsername)) throw new ArgumentNullException("newUsername");
            if (!_customerSettings.UsernamesEnabled) throw new ResearchException("Usernames are disabled");
            if (!_customerSettings.AllowUsersToChangeUsernames) throw new ResearchException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();
            if (newUsername.Length > _customerSettings.UsernameMaxLength)
                throw new ResearchException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameTooLong", false));

            var another = _customerService.GetCustomerByUsername(newUsername);
            if(another != null && another.Id != customer.Id)
                throw new ResearchException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameAlreadyExists", false));

            if (newUsername.Equals(customer.Username, StringComparison.InvariantCulture))
                return;

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }

        #endregion
    }
}
