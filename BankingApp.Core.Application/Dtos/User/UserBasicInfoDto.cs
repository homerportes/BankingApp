using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserBasicInfoDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string DocumentId { get; set; }
    }
}
