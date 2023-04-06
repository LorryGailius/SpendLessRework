using Blazored.LocalStorage;
using SpendLess.Client.Services;
using SpendLess.Server.Services;
using SpendLess.Shared;
using SpendLess.UnitTests.MockingServices;
using Moq;
using UnitTests.MockingServices;
using Microsoft.AspNetCore.Components.Authorization;
using SpendLess.Client;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SpendLess.Server.Models;
using Serilog;
using IInvocation = Castle.DynamicProxy.IInvocation;
using SpendLess.Server.Interceptor;
using SpendLess.Server.Middleware.Decorators;
using SpendLess.Server.Middleware;
using Microsoft.Extensions.Caching.Distributed;
using SpendLess.Server.Extensions;
using Tests.MockingServices;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace SpendLess.UnitTests
{
    public class Tests
    {

        private AuthServices _serverAuthServices;
        private AuthenticationService _clientAuthServices;

        private IHttpClientFactory _clientFactoryLogin = new HttpClientFactoryMock<LoginResponse>(
            new LoginResponse("success", "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJqb25hc0BnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTY2ODU1MDIxNX0.WRDJFuhBLpbeWK8q4fx4CNJ9TOZeq_owRbuuqt8CQrt-97ctoAfTff2vdyCBpgeXWY8bPW0sMJWVmZu5Q2fMWA"));
        private IHttpClientFactory _clientFactoryInt = new HttpClientFactoryMock<int>(0);
        private IHttpClientFactory _clientFactoryTransactionList = new HttpClientFactoryMock<List<Transactions?>>
        ( new List<Transactions?>{new Transactions(null, 10, "Food", DateTime.Now, "Taco", null), null} );


        private Microsoft.Extensions.Configuration.IConfiguration _config;
        private ILocalStorageService _localStorage = new LocalStorage();
        private ILocalStorageService _localStorageAuth = new LocalStorage();
        private AuthenticationStateProvider _authProviderMock = new CustomAuthStateProviderMock();
        private TransactionService _transactionServiceDelete;
        private TransactionService _transactionServiceAdd;
        private TransactionService _transactionServiceAddPeriodic;
        private TransactionService _transactionServiceGetTransactions;
        private PageService _pageService;
        private GoalService _goalService;
        private AuthenticationStateProvider _authStateProvider;
        private SpendLessContext _spendLessContext;
        private ISnackBarService _snackBarService;
        private Mock<ISnackBarService> _snackBarServiceMock;
        private Mock<IDatabaseService> databaseServiceMock;
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
        private Mock<HttpContext> _contextMock;
        DefaultHttpContext httpcontext = new DefaultHttpContext();
        private Mock<IDistributedCache> _cacheMock;
        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<IDistributedCache>();
           // _cacheMock.Setup(x => x.GetCacheValueAsync<ClientStatistics>(It.IsAny<string>(), default).Returns(Task.FromResult(new ClientStatistics())));
            var metadata = new EndpointMetadataCollection(new LimitRequests()
            {
                TimeWindow = 10,
                MaxRequests = 10
            });
            var endpoint = new Endpoint(null, metadata, null);

            httpcontext.SetEndpoint(endpoint);
            //_contextMock = new Mock<HttpContext>();
            //_contextMock.Setup(x => x.GetEndpoint()).Returns(endpoint);

            var key = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
            key.Setup(x => x.Value).Returns("my top secret key");
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _configurationMock.Setup(x => x.GetSection("AppSettings:Token")).Returns(key.Object);
            databaseServiceMock = new Mock<IDatabaseService>();
            databaseServiceMock.Setup(x => x.FindEmail("existing email")).Returns(Task.FromResult(true));
            databaseServiceMock.Setup(x => x.FindEmail("not existing email")).Returns(Task.FromResult(false));
            databaseServiceMock.Setup(x => x.AddNewUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            databaseServiceMock.Setup(x => x.GetUserPasswordSaltAsync(It.IsAny<UserDto>())).Returns(Task.FromResult(new byte[0]));
            databaseServiceMock.Setup(x => x.GetUserPasswordHashAsync(It.IsAny<UserDto>())).Returns(Task.FromResult(new byte[0]));
            databaseServiceMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            databaseServiceMock.Setup(x => x.AddTransaction(It.IsAny<Transactions>())).Returns(Task.CompletedTask);
            databaseServiceMock.Setup(x => x.GetUser(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            databaseServiceMock.Setup(x => x.RemoveTransaction(It.IsAny<Transactions>())).Returns(Task.CompletedTask);

            var mockTransactions = new List<Transactions>();
            mockTransactions.Add(new Transactions(1, 1, "", DateTime.MinValue));
            databaseServiceMock.Setup(x => x.GetTransactionsAsync(It.IsAny<int>())).Returns(Task.FromResult(mockTransactions));


            Log.Logger = new LoggerConfiguration()
                 .WriteTo.File(Environment.CurrentDirectory + "\\Logs\\exceptions-.log", rollingInterval: RollingInterval.Day)
                 .CreateLogger();
            _snackBarServiceMock = new Mock<ISnackBarService>();
            _snackBarServiceMock.Setup(service => service.SuccessMsg(It.IsAny<string>())).Verifiable();
            _snackBarServiceMock.Setup(service => service.ErrorMsg(It.IsAny<string>())).Verifiable();
            _snackBarServiceMock.Setup(service => service.WarningMsg(It.IsAny<string>())).Verifiable();



            _authStateProvider = new CustomAuthenticationStateProvider(_localStorageAuth, _clientFactoryLogin);
            _transactionServiceAddPeriodic = new TransactionService(_clientFactoryTransactionList, _localStorage, _authProviderMock, _snackBarServiceMock.Object);
            _transactionServiceAdd = new TransactionService(_clientFactoryInt, _localStorage, _authProviderMock, _snackBarServiceMock.Object);
            _transactionServiceDelete = new TransactionService(_clientFactoryLogin, _localStorage, _authProviderMock, _snackBarServiceMock.Object);
            _transactionServiceGetTransactions = new TransactionService(_clientFactoryTransactionList, _localStorage, _authProviderMock, _snackBarServiceMock.Object);
            _serverAuthServices = new AuthServices(null);
            _clientAuthServices = new AuthenticationService(_clientFactoryLogin, _localStorage, _authProviderMock, _snackBarServiceMock.Object);
            _pageService = new PageService(_transactionServiceDelete);
            _goalService = new GoalService(_clientFactoryLogin, _localStorage, _authProviderMock, _snackBarServiceMock.Object);

        }



        [Test]
        public async Task AllowedAmountOfRequestsInvokesNextDelegate()
        {
            string status = "initial";
            RequestDelegate next = (httpcontext) => Task.Run(() => status = "changed");
            RateLimitingMiddleware middleware = new RateLimitingMiddlewareMock(next, _cacheMock.Object, null);
            await middleware.InvokeAsync(httpcontext);
            Task.WaitAny();
            Assert.That(status == "changed");
        }

        [Test]
        public async Task TooManyRequestsWhenclientStatisticsIsNotNull()
        {
            var value = new ClientStatistics()
            {
                LastSuccessfulResponseTime = DateTime.UtcNow,
                NumberOfRequestsCompletedSuccessfully = 10
            };
            RateLimitingMiddleware middleware = new RateLimitingMiddlewareMock(null, _cacheMock.Object, value);
            await middleware.InvokeAsync(httpcontext);
            Assert.That(httpcontext.Response.StatusCode == (int)HttpStatusCode.TooManyRequests);
        }


        [Test]
        public  void DeserialisingObjectReturnsObject ()
        {        
            var serialized = Encoding.Default.GetBytes(JsonConvert.SerializeObject("word"));
            Assert.That(serialized.FromByteArray<string>(), Is.EqualTo("word"));
        }

        [Test]
        public void SerialisingObjectReturnsBiteArray()
        {          
            var word = "word";
            var serialized = Encoding.Default.GetBytes(JsonConvert.SerializeObject(word));
            Assert.That(word.ToByteArray(), Is.EqualTo(serialized));
        }


        [Test]
        public async Task SuccessfulyCreatingAccountInDatabaseReturnsTrue()
        {
            var authService = new AuthServices(databaseServiceMock.Object);
            var user = new UserDto()
            {
                Email = "not existing email",
                Password = "password",
                Username = "username"
            };
            Assert.That(await authService.CreateAccount(user) is true);
        }

        [Test]
        public async Task RegisteringWithNewAccountInDatabaseReturnsLoginResponseWithSuccessMessage()
        {
            var authService = new AuthServices(databaseServiceMock.Object);
            var user = new UserDto()
            {
                Email = "not existing email",
                Password = "password",
                Username = "username"
            };
            var loginResponse = await authService.Register(user, _configurationMock.Object);
            Assert.That(loginResponse.message == "Success");
        }

        [Test]
        public async Task RegisteringWithExistingAccountInDatabaseReturnsLoginResponseWithNullToken()
        {
            var authService = new AuthServices(databaseServiceMock.Object);
            var user = new UserDto()
            {
                Email = "existing email",
                Password = "password",
                Username = "username"
            };
            var loginResponse = await authService.Register(user, _configurationMock.Object);
            Assert.That(loginResponse.token == null);
        }

        [Test]
        public async Task LoggingToNonExistingAccountInDatabaseReturnsLoginResponseWithNullToken()
        {
            var authService = new AuthServices(databaseServiceMock.Object);
            var user = new UserDto()
            {
                Email = "not existing email",
                Password = "password",
                Username = "username"
            };
            var loginResponse = await authService.Login(user, _configurationMock.Object);
            Assert.That(loginResponse.token == null);
        }

        [Test]
        public async Task NewlyAddedTransactionReceivesId()
        {
            var service = new TransactionsService(databaseServiceMock.Object);
            var transaction = new Transactions(-1, 1, "", DateTime.UtcNow);

            var identity = httpcontext.User.Identities.FirstOrDefault();
            identity.AddClaim(new Claim(ClaimTypes.Email, "email"));


            int? result = await service.AddTransaction(transaction, _spendLessContext, httpcontext);
            Assert.That(result.HasValue);
        }

        [Test]
        public async Task AddingPeriodicTransactionsToDatabase()
        {
            var service = new TransactionsService(databaseServiceMock.Object);
            var transaction = new Transactions(-1, 1, "", DateTime.UtcNow);
            var transactions = new List<Transactions>();
            transactions.Add(transaction);

            var identity = httpcontext.User.Identities.FirstOrDefault();
            identity.AddClaim(new Claim(ClaimTypes.Email, "email"));


            var result = await service.AddPeriodicTransaction(transactions, _spendLessContext, httpcontext);
            Assert.That((result.ElementAt(0)) == transaction);
        }

        [Test]
        public async Task GetTransactionListFromDatabaseByUserId()
        {
            var service = new TransactionsService(databaseServiceMock.Object);

            var identity = httpcontext.User.Identities.FirstOrDefault();
            identity.AddClaim(new Claim(ClaimTypes.Email, "email"));


            var result = await service.GetTransactions(_spendLessContext, httpcontext);
            var firstTrn = result.ElementAt(0);
            Assert.That(firstTrn.Id == 1 && firstTrn.TransactionDate == DateTime.MinValue);
        }

        [Test]
        public async Task DeleteTransactionFromDatabaseReturnSuccess()
        {
            var service = new TransactionsService(databaseServiceMock.Object);

            var result = await service.DeleteTransaction(1, _spendLessContext);
            
            Assert.That(result);
        }

        [Test]
        public async Task DeleteTransactionFromDatabaseWithIncorrectIdFails()
        {
            var service = new TransactionsService(databaseServiceMock.Object);

            var result = await service.DeleteTransaction(-1, _spendLessContext);

            Assert.That(!result);
        }

        [Test]
        public void VoidMethodsWithExceptionsAreLoggedByInterceptor()
        {
            var file = new DirectoryInfo(Environment.CurrentDirectory + "\\Logs")
                .GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
            long lengthInitial = file.Length;

            var invocation = new Mock<IInvocation>();
            invocation.Setup(service => service.Proceed()).Throws<Exception>();
            var interceptorMock = new UnhandledExceptionLogger();
            Assert.Throws<Exception>(() => interceptorMock.Intercept(invocation.Object));
            file.Refresh();
            long lengthFinal = file.Length;
            Assert.That(lengthFinal > lengthInitial);

        }

        [Test]
        public void TaskMethodsWithExceptionsAreLoggedByInterceptor()
        {
            var task = ((Func<Task>)(async () => {
                throw new Exception();
            }))();
            var invocation = new Mock<IInvocation>();
            invocation.Setup(service => service.Method).Throws<Exception>();
            var interceptorMock = new UnhandledExceptionLogger();
            Assert.ThrowsAsync<Exception>(async () => await interceptorMock.InterceptAsync(task));

        }

        [Test]
        public async Task TaskResultMethodsWithExceptionsAreLoggedByInterceptor()
        {
            var task = ((Func<Task<string>>)(async () => {
                 throw new Exception("TaskResult");
            }))();
            var invocation = new Mock<IInvocation>();
            invocation.Setup(service => service.Method).Throws<Exception>();
            var interceptorMock = new UnhandledExceptionLogger();
            Assert.ThrowsAsync<Exception>(async () => await interceptorMock.InterceptAsync(task));
        }

        [Test]
        public async Task LoggingExceptionsToFile()
        {
            var file = new DirectoryInfo(Environment.CurrentDirectory + "\\Logs")
                .GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
            long lengthInitial = file.Length;
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, "email"),
                new Claim(ClaimTypes.Expiration, "date")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPirncipal = new ClaimsPrincipal(identity);
            var service = new ExceptionService();
            await service.LogException(new Exception(), claimsPirncipal);
            file.Refresh();
            long lengthFinal = file.Length;
            Assert.That(lengthFinal > lengthInitial);
        }

        [Test]
        public async Task LoggingExceptionsToFileWhenExceptionIsThrownInMethod()
        {
            var service = new ExceptionService();
            Assert.ThrowsAsync<NullReferenceException>(async () => await service.LogException(new Exception(), null));

        }

        [TestCase(true)]
        [TestCase(false)]
        [Test]
        public async Task ValidatingUserReturnsTrueIfUserWasFoundInDatabase(bool isSuccess)
        {
          
            
            var success = await _clientAuthServices.ValidateLogin(isSuccess, "email", "user", "psw");
            Assert.That(success, Is.EqualTo(isSuccess));

        }

        [Test]
        public async Task GettingTransactionsSetsTransactionList()
        {
            var list1 = _transactionServiceGetTransactions.Transactions;
            await _transactionServiceGetTransactions.GetTransactions();
            var list2 = _transactionServiceGetTransactions.Transactions;
            Task.WaitAny();
            Assert.That(list2.Count() > 0);

        }


        [TestCase(true, null, false)]
        [TestCase(false, "something", false)]
        [Test]
        public async Task SortsListWhenNewValueIsAdded(bool toggleExpenseIncome, string? textValue, bool togglePeriodical)
        {
            await _transactionServiceAdd.Savelist(10, toggleExpenseIncome, textValue, "Food", DateTime.Now, togglePeriodical, 0, "year", DateTime.Now);
            Task.WaitAll();
            var list = _transactionServiceAdd.Transactions;
             
            Assert.That(list.Count() > 0);
        }

        [Test]
        public void ParsingJwtReturnsIEnumerableWithClaims()
        {
            string token = "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJqb25hc0BnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTY2ODU1MDIxNX0.WRDJFuhBLpbeWK8q4fx4CNJ9TOZeq_owRbuuqt8CQrt-97ctoAfTff2vdyCBpgeXWY8bPW0sMJWVmZu5Q2fMWA";
            var state = CustomAuthenticationStateProvider.ParseClaimsFromJwt(token);
            var email = state.Select(s => s.Value).FirstOrDefault();
            Assert.That(email, Is.EqualTo("jonas@gmail.com"));

        }



        [Test]
        public async Task GettingAuthenticationStateWithExpiredTokenNonAuthentificatedState()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = state.User.Identity.IsAuthenticated;
            Assert.False(isAuthenticated);
        }

        [Test]
        public async Task AddingTransactionsListChangesTransactionList()
        {
            var listCountBefore = _transactionServiceAddPeriodic.Transactions.Count;
            await _transactionServiceAddPeriodic.AddPeriodicTransaction(10, "Food", DateTime.Now, "Pizza", "week(s)", 3, DateTime.Now.AddMonths(2));
            var listCountAfter = _transactionServiceAddPeriodic.Transactions.Count;
            Assert.True(listCountBefore < listCountAfter);
        }

        [Test]
        public async Task GettingTransactionsListChangesTransactionList()
        {
            var listCountBefore = _transactionServiceAddPeriodic.Transactions.Count;
            await _transactionServiceAddPeriodic.GetTransactions();
            var listCountAfter = _transactionServiceAddPeriodic.Transactions.Count;
            Assert.True(listCountBefore < listCountAfter);
        }

        [Test]
        public async Task SuccessfulyCreatingAccountReturnsTrue()
        {
            var result = await _clientAuthServices.CreateAccount(new UserDto()
            {
                Email = "name@gmail.com",
                Password = "password",
                Username = "username"
            });
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SuccessfulyCreatingAccountAddsTokenToLocalStorage()
        {
            var result = await _clientAuthServices.CreateAccount(new UserDto()
            {
                Email = "name@gmail.com",
                Password = "password",
                Username = "username"
            });

            Assert.IsNotNull(_localStorage.GetItemAsStringAsync("token"));
        }

        [Test]
        public async Task CheckingForExistingAccountReturnsTrue()
        {
            var result = await _clientAuthServices.GetLoginAuthentication(new UserDto()
            {
                Email = "name@gmail.com",
                Password = "password",
            });
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeletingTransactionReturnsMessage()
        {

            Assert.That(await _transactionServiceDelete.DeleteTransaction(5),
                Is.EqualTo("Transaction was successfully deleted"));
        }

        [Test]
        public async Task AddingTransactionAppendsToList()
        {
            int count = _transactionServiceAdd.Transactions.Count;
            await _transactionServiceAdd.AddTransaction(50, "Food", DateTime.Now, "Taco");

            Transactions item = _transactionServiceAdd.Transactions.Last();

            Assert.That(_transactionServiceAdd.Transactions.Count, Is.EqualTo(count + 1));
        }

        [Test]
        public async Task ChangingCurrentGoalAmountReturnSuccess()
        {
            var result = _goalService.ChangeCurrentAmount(new Goal(1, 1, "", 5, DateTime.MinValue, 0));

            Assert.That(result.Result.Equals("Successfully Added value"));
        }

        [Test]
        public async Task RemovingTransactionWithPageServiceRemovesFromList()
        {
            var listCountBefore = _transactionServiceAddPeriodic.Transactions.Count;
            await _transactionServiceAddPeriodic.AddPeriodicTransaction(10, "Food", DateTime.Now, "Pizza", "week(s)", 3, DateTime.Now.AddMonths(2));
            await _pageService.DeleteRow(10);
            var listCountAfter = _transactionServiceAddPeriodic.Transactions.Count;
            Assert.True(listCountBefore < listCountAfter);
        }

        [Test]
        public void ProvidingInvalidRequestReturnsFalse()
        {
            Assert.IsFalse(_serverAuthServices.VerifyRequest(null));
        }

        [Test]

        public void CreatingTokenWhenArgumentsAreInvalidReturnsNull()
        {
            var user = new UserDto
            {
                Email = "email@gmail.com",
                Password = "password",
                Username = "username"
            };
            Assert.Throws<NullReferenceException>(() => _serverAuthServices.CreateToken(user, null));
        }

        [Test]
        public async Task SearchingForCategoryValueWithPartialInput()
        {
            var result = await _pageService.Search("Trans");
            string resultString = result.ElementAt(0);

            Assert.That("Transportation".Equals(resultString));
        }

        [Test]
        public async Task GetDatesReturnProperResult()
        {
            var result = _pageService.GetDates(DateTime.MinValue);

            Assert.That(result.Count == 31);
        }

        [Test]
        public void CreatingHashedPasswordReturnsTwoBiteArrays()
        {
            var password = "password";

            _serverAuthServices.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            Assert.IsInstanceOf<byte[]>(passwordHash);
            Assert.IsInstanceOf<byte[]>(passwordSalt);
        }

        [Test]
        public void ProvidingEmptyPasswordReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckPassword(""), Is.EqualTo("Password is required!"));

        }

        [Test]
        public void ProvidingTooShortPasswordReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckPassword("short"),
                Is.EqualTo("Password must be at least of length 8"));
        }

        [Test]
        public void ProvidingPasswordWithoutCapitalLettersReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckPassword("no_capital"),
                Is.EqualTo("Password must contain at least one capital letter"));

            Assert.That(_clientAuthServices.CheckPassword("NO_LOWERCASE"),
                Is.EqualTo("Password must contain at least one lowercase letter"));

            Assert.That(_clientAuthServices.CheckPassword("No_Digits"),
                Is.EqualTo("Password must contain at least one digit"));
        }

        [Test]
        public void ProvidingPasswordWithoutLowercaseLettersReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckPassword("NO_LOWERCASE"),
                Is.EqualTo("Password must contain at least one lowercase letter"));

            Assert.That(_clientAuthServices.CheckPassword("No_Digits"),
                Is.EqualTo("Password must contain at least one digit"));
        }

        [Test]
        public void ProvidingPasswordWithoutDigitsReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckPassword("No_Digits"),
                Is.EqualTo("Password must contain at least one digit"));
        }

        [Test]
        public void ProvidingEmptyEmailReturnsMessage()
        {
            Assert.That(_clientAuthServices.CheckEmail(""),
                Is.EqualTo("Email is required"));

        }

        [Test]
        [TestCase("email")]
        [TestCase("email@")]
        [TestCase("email@com")]
        [TestCase("@gmail.com")]
        public void ProvidingInvalidEmailReturnsMessage(string email)
        {

            Assert.That(_clientAuthServices.CheckEmail(email)!,
                Is.EqualTo("Email format is incorrect"));

        }
    }
}