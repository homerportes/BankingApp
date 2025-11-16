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
        public async Task<LoanPaginationResultDto> GetAllFiltered(
        int page = 1,
        int pageSize = 20,
        string? state = null,
        string? clientId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _repo.GetAllQueryWithInclude(new List<string> { "Installments" });

            if (!string.IsNullOrEmpty(state))
            {
                try
                {
                    var statusEnum = EnumMapper<LoanStatus>.FromString(state);
                    query = query.Where(r => r.Status == statusEnum);
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                query = query.Where(r => r.ClientId == clientId);
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
                CurrentPage = page,
            };
        }


        public async Task<decimal> GetTotalLoanDebt()
        {
            return await _repo
                .GetAllQuery()
                .Where(r => r.IsActive)
                .SumAsync(r => r.OutstandingBalance);
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
                        loan.Status = LoanStatus.DELIQUENT;

                    await _repo.UpdateByObjectAsync(loan);

                    totalProcessed += loans.Count();
                    skip += batchSize;

                    _logger.LogInformation("Procesados {count} préstamos hasta ahora.", totalProcessed);
                }
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

     
        public static decimal DecimalPow(decimal baseValue, int exponent)
        {
            if (exponent < 0)
                throw new ArgumentException("El exponente debe ser >= 0");

            decimal result = 1m;
            decimal factor = baseValue;

            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    result *= factor;

                factor *= factor;
                exponent >>= 1;
            }

            return result;
        }

        public static decimal CalculateInstallment(decimal principal, decimal annualRate, int months)
        {
            decimal monthlyRate = (annualRate / 100m) / 12m;

            if (monthlyRate == 0)
                return Math.Round(principal / months, 2, MidpointRounding.ToEven);

            decimal pow = DecimalPow(1 + monthlyRate, months);

            decimal value = principal * (monthlyRate * pow) / (pow - 1);

            return Math.Round(value, 2, MidpointRounding.ToEven);
        }


        public async Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate)
        {
            var result = new OperationResultDto { IsSuccessful = false };

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

            var pendingInstallments = loan.Installments!
                .Where(i => !i.IsPaid && i.PayDate > DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(i => i.Number)
                .ToList();

            int remainingMonths = pendingInstallments.Count;

            if (remainingMonths == 0)
            {
                result.StatusMessage = "No hay cuotas pendientes para recalcular.";
                return result;
            }

            decimal principalPending = loan.OutstandingBalance;

            decimal newInstallmentValue = CalculateInstallment(
                principal: principalPending,
                annualRate: newRate,
                months: remainingMonths
            );

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                loan.InterestRate = newRate;
                loan.UpdatedAt = DateTime.Now;

                foreach (var installment in pendingInstallments)
                {
                    installment.Value = newInstallmentValue;
                    installment.IsModified = true;
                }

                await _repo.UpdateByObjectAsync(loan);
                await _installmentRepo.UpdateRangeAsync(pendingInstallments);

                await _unitOfWork.CommitAsync();

                result.IsSuccessful = true;
                result.StatusMessage = $"Tasa actualizada correctamente. Cada cuota pendiente ahora es {newInstallmentValue:C}.";

                // Enviar correo al cliente
                var user = await _UserService.GetUserById(loan.ClientId);

                string emailBody = $@"
<html>
    <body>
        <p>Estimado/a {user!.Name??""} {user.LastName},</p>
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

            decimal constantPayment = CalculateInstallment(
                request.LoanAmount,
                request.AnualInterest,
                request.LoanTermInMonths
            );

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var loanEntity = new Loan
                {
                    Id = Guid.NewGuid(),
                    ClientId = request.ClientId,
                    LoanTermInMonths = request.LoanTermInMonths,
                    InterestRate = request.AnualInterest,
                    OutstandingBalance = request.LoanAmount,
                    TotalLoanAmount = request.LoanAmount,
                    PublicId = await GenerateLoanId(),
                    Status = LoanStatus.ONTIME,
                    IsActive = true,
                    CreatedAt = now
                };

                await _repo.AddAsync(loanEntity);

                var installments = new List<Installment>(request.LoanTermInMonths);
                var firstDueDate = DateOnly.FromDateTime(now.AddMonths(1));

                for (int i = 0; i < request.LoanTermInMonths; i++)
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

                await _transacctionRepository.AddAsync(new Transaction
                {
                    AccountId = account!.Id,
                    AccountNumber = account.Number,
                    Type = TransactionType.CREDIT,
                    Status = OperationStatus.APPROVED,
                    Amount = request.LoanAmount,
                    DateTime = DateTime.Now,
                    Beneficiary = account.Number,
                    Origin = "SYSTEM",
                    Id = Guid.NewGuid()
                });

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
                <p>Estimado/a {user!.Name??""} {user.LastName},</p>
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
