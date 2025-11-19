using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace BankingApp.Core.Application.Services
{
    public abstract class BaseLoanService : GenericService<Loan, LoanDto>, IBaseLoanService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<Loan> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoanRepository _repo;
        private readonly IInstallmentRepository _installmentRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IUserService _UserService;
        private readonly ICreditCardRepository _cardRepository;
        private readonly ITransacctionRepository _transacctionRepository;

        public BaseLoanService(
            ILoanRepository repo,
            IMapper mapper,
            ILogger<Loan> logger,
            IUnitOfWork unitOfWork,
            IInstallmentRepository installmentRepository,
            IAccountRepository accountRepository,
            IEmailService emailService,
            IUserService userService,
            ICreditCardRepository cardRepository,
            ITransacctionRepository transacctionRepository
        ) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _installmentRepo = installmentRepository;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _UserService = userService;
            _cardRepository = cardRepository;
            _transacctionRepository = transacctionRepository;
        }

        public async Task<string> GenerateLoanId()
        {
            bool LoanIdExists = false;
            string Id;
            do
            {
                Id = new string(
                    Guid.NewGuid()
                    .ToString("N")
                    .Where(char.IsDigit)
                    .Take(9)
                    .ToArray()
                );

                LoanIdExists = await _repo.LoanPublicIdExists(Id);

            } while (LoanIdExists);

            return Id;
        }





           public async Task<LoanPaginationResultDto> GetAllFilteredAPI(
           int page = 1,
           int pageSize = 20,
           string? state = null,
           string? clientId = null)
        {
            if (clientId == "__NO_MATCH__")
            {
                return new LoanPaginationResultDto
                {
                    Data = new List<LoanDto>(),
                    PagesCount = 0,
                    CurrentPage = page
                };
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _repo.GetAllQueryWithInclude(new List<string> { "Installments" });

            if (!string.IsNullOrWhiteSpace(state))
            {
                var normalized = state.Trim().ToLower();

                foreach (var enumValue in Enum.GetValues(typeof(LoanStatus)))
                {
                    var enumString = EnumMapper<LoanStatus>
                        .ToString((LoanStatus)enumValue)?
                        .ToLower();

                    if (enumString == normalized)
                    {
                        query = query.Where(r => r.Status == (LoanStatus)enumValue);
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var cleanClientId = clientId.Trim();
                query = query.Where(r => r.ClientId == cleanClientId);
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync();

            var userIds = data.Select(x => x.ClientId).Distinct().ToList();
            var users = await _UserService.GetUsersBasicInfoAsync(userIds);
            var usersDict = users.ToDictionary(u => u.Id);

            var mapped = _mapper.Map<List<LoanDto>>(data);

            foreach (var loan in mapped)
            {
                if (usersDict.TryGetValue(loan.ClientId, out var user))
                {
                    loan.ClientName = user.FullName;
                    loan.ClientDocumentIdNumber = user.DocumentId;
                    loan.LoanStatus = EnumMapper<LoanStatus>.ToString(loan.Status);
                }
            }

            return new LoanPaginationResultDto
            {
                Data = mapped,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page
            };
        }

        public async Task<decimal> GetAverageSystemDebt()
        {
            var clientIds = await _UserService.GetAllClientIds();

            if (clientIds == null || clientIds.Count == 0)
                return 0;

            var totalDebt = await _repo.GetAllQuery()
                .Where(r => clientIds.Contains(r.ClientId))
                .SumAsync(r => r.OutstandingBalance);

            return totalDebt / clientIds.Count;
        }


        public async Task<decimal> GetTotalLoanDebt()
        {

           

            return await _repo.GetAllQuery().Where(r => r.IsActive).SumAsync(r => r.OutstandingBalance);


          

        }


        public async Task<decimal> GetAverageLoanDebt()
        {
            var Clients = await _UserService.GetAllClientIds();
            return await _repo.GetAllQuery().Where(r => r.IsActive).SumAsync(r => r.OutstandingBalance) / Clients.Count();



        }


        public async Task<bool> ClientHasActiveLoan(string clientId)
        {
            return await _repo
                .GetAllQuery()
                .Where(r => r.IsActive && r.ClientId == clientId)
                .AnyAsync();
        }


        public async Task<decimal> GetClientLoansDebt(string clientId)
        {

            return await _repo
                .GetAllQuery()
                .Where(r => r.IsActive && r.ClientId == clientId)
                .Select(r => r.OutstandingBalance)
                .SumAsync();


        }




    

        public async Task VerifyAndMarkDelayedLoansAsync()
        {
            const int batchSize = 200;
            int skip = 0;
            int totalProcessed = 0;

            while (true)
            {
                var loans = await _repo
                    .GetAllQueryWithInclude(new List<string> { "Installments" })
                    .Where(l => l.Installments!.Any(i => !i.IsPaid && i.PayDate < DateOnly.FromDateTime(DateTime.Now)))
                    .OrderBy(l => l.CreatedAt)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!loans.Any()) break;

                foreach (var loan in loans)
                {
                    bool hasDelay = false;

                    foreach (var installment in loan.Installments!)
                    {
                        if (!installment.IsPaid && installment.PayDate < DateOnly.FromDateTime(DateTime.Now))
                        {
                            installment.IsDelinquent = true;
                            hasDelay = true;
                        }
                    }

                    if (hasDelay)
                    {
                        loan.Status = LoanStatus.DELIQUENT;
                        await _repo.UpdateByObjectAsync(loan);
                    }
                }

                totalProcessed += loans.Count;
                skip += loans.Count;

                _logger.LogInformation("Procesados {count} préstamos hasta ahora.", totalProcessed);
            }
        }
        public async Task<DetailedLoanDto?> GetDetailed(string Id)
        {
            var loan = await _repo
                .GetAllQueryWithInclude(new List<string> { "Installments"})
                .Where(r => r.PublicId == Id)
                .FirstOrDefaultAsync();

            if (loan == null) return null;

            return new DetailedLoanDto
            {
                LoadId = loan.PublicId,
                Installments = _mapper.Map<List<InstallmentDto>>(loan.Installments)
            };
        }


        public async Task<decimal> GetLoanRate(string Id)
        {
            return await _repo
                .GetAllQuery()
                .Where(r => r.PublicId == Id)
                .Select(r => r.InterestRate).FirstAsync();

       
        }

        public static decimal DecimalPow(decimal baseValue, int exponent)
        {
            if (exponent < 0) throw new ArgumentException("El exponente debe ser >= 0");

            decimal result = 1m;
            decimal factor = baseValue;

            while (exponent > 0)
            {
                if ((exponent & 1) == 1) result *= factor;
                factor *= factor;
                exponent >>= 1;
            }

            return result;
        }




        public static decimal CalculateInstallment(decimal principal, decimal annualRatePercent, int months)
        {
            if (months <= 0) throw new ArgumentException("months debe ser > 0");

            decimal monthlyRate = (annualRatePercent / 100m) / 12m;

            if (monthlyRate == 0m)
            {
                decimal plain = principal / months;
                return Math.Round(plain, 2, MidpointRounding.ToEven);
            }

            decimal onePlusRPowN = DecimalPow(1m + monthlyRate, months);
            decimal numerator = principal * (monthlyRate * onePlusRPowN);
            decimal denominator = onePlusRPowN - 1m;

            decimal value = numerator / denominator;

            return Math.Round(value, 2, MidpointRounding.ToEven);
        }

        public async Task<CreateLoanResult> HandleCreateRequest(LoanRequest request)
        {
            var result = new CreateLoanResult
            {
                ClientIsHighRisk = false,
                ClientIsAlreadyHighRisk = false,
                ClientHasActiveLoan = false
            };

            if (request == null ||
                string.IsNullOrEmpty(request.ClientId) ||
                request.LoanAmount <= 0 ||
                request.AnualInterest <= 0 ||
                request.LoanTermInMonths <= 0)
            {
                result.HasValidationErrors = true;
                result.ValidationMessage = "Datos incompletos o inválidos.";
                return result;
            }

            result.ClientHasActiveLoan = await ClientHasActiveLoan(request.ClientId);
            if (result.ClientHasActiveLoan)
                return result;

            var clientLoansDebt = await GetClientLoansDebt(request.ClientId);
            var cardDebt = await _cardRepository.GetClientTotalCreditCardDebt(request.ClientId);
            var userDebt = clientLoansDebt + cardDebt;

            var systemLoansDebt = await GetTotalLoanDebt();
            var systemCardDebt = await _cardRepository.GetTotalClientsCreditCardDebt();
            var clientsCount = await _UserService.GetAllClientsCount();

            var systemDebt = (systemLoansDebt + systemCardDebt) / clientsCount;

            if (userDebt > systemDebt)
            {
                result.ClientIsAlreadyHighRisk = true;
                return result;
            }

            decimal cuota = CalculateInstallment(
                request.LoanAmount,
                request.AnualInterest,
                request.LoanTermInMonths
            );

            decimal newLoanTotal = cuota * request.LoanTermInMonths;
            newLoanTotal = Math.Round(newLoanTotal, 2);

            bool becomesHighRisk = (userDebt + newLoanTotal) > systemDebt && systemDebt>0
                ;

            result.ClientIsHighRisk = becomesHighRisk;

            if (becomesHighRisk)
                return result;

            var createResult = await Create(request);

            if (createResult.LoanCreated)
                await SendEmail(request, createResult);

            return createResult;
        }







        public async Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate)
        {
            var result = new OperationResultDto { IsSuccessful = false };

            // Obtener préstamo con cuotas
            var loan = await _repo
                .GetAllQueryWithInclude(new List<string> { "Installments" })
                .FirstOrDefaultAsync(l => l.PublicId == publicId);

            if (loan is null)
            {
                result.StatusMessage = "Préstamo no encontrado.";
                return result;
            }

            if (!loan.IsActive)
            {
                result.StatusMessage = "No se puede actualizar un préstamo inactivo.";
                return result;
            }

            int n = loan.LoanTermInMonths;
            decimal P = loan.Amount; // capital inicial
            decimal monthlyRate = (newRate / 100m) / 12m;

            // Calcular nueva cuota mensual
            decimal onePlusRPowN = DecimalPow(1m + monthlyRate, n);
            decimal newInstallmentValue = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1m);
            newInstallmentValue = Math.Round(newInstallmentValue, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Actualizar tasa y fecha
                loan.InterestRate = newRate;
                loan.UpdatedAt = DateTime.Now;

                // Recalcular cuotas pendientes
                foreach (var installment in loan.Installments!.Where(i => !i.IsPaid))
                {
                    installment.Value = newInstallmentValue;
                    installment.IsModified = true;
                }

                // Actualizar saldo pendiente
                loan.OutstandingBalance = loan.Installments
                    .Where(i => !i.IsPaid)
                    .Sum(i => i.Value);

                // Guardar cambios
                await _repo.UpdateByObjectAsync(loan);
                await _installmentRepo.UpdateRangeAsync(loan.Installments!.ToList());

                await _unitOfWork.CommitAsync();

                result.IsSuccessful = true;
                result.StatusMessage = $"Tasa actualizada correctamente. Cada cuota pendiente ahora es {newInstallmentValue:C}.";

                // Enviar correo al cliente
                var user = await _UserService.GetUserById(loan.ClientId);

                string emailBody = $@"
<html>
    <body>
        <p>Estimado/a {user.Name} {user.LastName},</p>
        <p>Su préstamo con ID {loan.PublicId} ha tenido una actualización en la tasa de interés.</p>
        <p>Nueva tasa anual: {newRate}%</p>
        <p>Valor actualizado de cada cuota pendiente: {newInstallmentValue:C}</p>
        <p>Por favor revise su plan de pagos actualizado.</p>
        <p>Atentamente,<br/>Banco XYZ</p>
    </body>
</html>";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Actualización de tasa de interés",
                    BodyHtml = emailBody
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar la tasa del préstamo {PublicId}", publicId);
                result.StatusMessage = "Ocurrió un error al actualizar la tasa.";
            }

            return result;
        }






        public async Task<CreateLoanResult> Create(LoanRequest request)
        {
            var result = new CreateLoanResult();

            int months = request.LoanTermInMonths;
            decimal constantPayment = CalculateInstallment(
                request.LoanAmount,
                request.AnualInterest,
                months
            );

            decimal totalToPay = constantPayment * months; 
            totalToPay = Math.Round(totalToPay, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var loanEntity = new Loan
                {
                    Id = Guid.NewGuid(),
                    ClientId = request.ClientId,
                    LoanTermInMonths = months,
                    InterestRate = request.AnualInterest,
                    OutstandingBalance = totalToPay,
                    Amount=request.LoanAmount,
                    TotalLoanAmount = totalToPay,     
                    PublicId = await GenerateLoanId(),
                    Status = LoanStatus.ONTIME,
                    IsActive = true,
                    CreatedAt = now
                };

                await _repo.AddAsync(loanEntity);

                var installments = new List<Installment>(months);
                var firstDueDate = DateOnly.FromDateTime(now.AddMonths(1));

                for (int i = 0; i < months; i++)
                {
                    var dueDate = firstDueDate.AddMonths(i);

                    installments.Add(new Installment
                    {
                        LoanId = loanEntity.Id,
                        Id = 0,
                        Number = i + 1,
                        Value = constantPayment,
                        PayDate = dueDate,
                        IsPaid = false,
                        IsDelinquent = dueDate < DateOnly.FromDateTime(now)
                    });
                }

                await _installmentRepo.AddRangeAsync(installments);

                var account = await _accountRepository
                    .GetAllQuery()
                    .Where(a => a.UserId == request.ClientId && a.Type == AccountType.PRIMARY)
                    .FirstOrDefaultAsync();

                if (account is not null)
                {
                    account.Balance += request.LoanAmount;
                    await _accountRepository.UpdateAsync(account.Id, account);
                }

                if (account is not null)
                {
                    await _transacctionRepository.AddAsync(new Transaction
                    {
                        AccountId = account.Id,
                        AccountNumber = account.Number,
                        Type = TransactionType.CREDIT,
                        OperationId = _transacctionRepository.GenerateOperationId(),
                        Status = OperationStatus.APPROVED,
                        Amount = request.LoanAmount,
                        DateTime = DateTime.Now,
                        Beneficiary = account.Number,
                        Origin = "SYSTEM",
                        Id = Guid.NewGuid()
                    });
                }

             

                await _unitOfWork.CommitAsync();

                result.LoanCreated = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al crear el préstamo para el cliente {ClientId}", request.ClientId);
                result.LoanCreated = false;
            }

            return result;
        }

        public async Task<CreateLoanResult> SendEmail(LoanRequest request, CreateLoanResult createLoanResult)
        {
            var user = await _UserService.GetUserById(request.ClientId);

            string emailBody = $@"
        <html>
            <body>
                <p>Estimado/a {user.Name} {user.LastName},</p>
                <p>Nos complace informarle que su solicitud de préstamo ha sido aprobada.</p>
                <table style='border-collapse: collapse;'>
                    <tr>
                        <td><strong>Monto del préstamo:</strong></td>
                        <td>{request.LoanAmount:C}</td>
                    </tr>
                    <tr>
                        <td><strong>Interés anual:</strong></td>
                        <td>{request.AnualInterest}%</td>
                    </tr>
                    <tr>
                        <td><strong>Plazo:</strong></td>
                        <td>{request.LoanTermInMonths} meses</td>
                    </tr>
                </table>
                <p>Le agradecemos por confiar en nosotros.</p>
                <p>Atentamente,<br/>Banco XYZ</p>
            </body>
        </html>";

            await _emailService.SendAsync(new EmailRequestDto()
            {
                Subject = "Asignación de Préstamo",
                To = user.Email,
                BodyHtml = emailBody
            });

            return createLoanResult;
        }
    }
}
