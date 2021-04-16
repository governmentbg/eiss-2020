using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IISPNCaseService : IBaseMQService
    {
        List<int> GetCaseIdSurroundSideErr();
        Task<bool> PatchISPN(int caseId);
        void SetMqIdForTest(int mqId);
    }
}

