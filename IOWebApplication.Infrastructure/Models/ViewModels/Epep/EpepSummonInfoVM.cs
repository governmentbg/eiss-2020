// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepSummonInfoVM
    {

        public bool CanSummonByEpep { get; set; }


        /// <summary>
        /// CasePerson.Id на лицето по делото, CaseSessionId==null
        /// </summary>
        /// <value>The case person identifier.</value>
        public int CasePersonId { get; set; }

        public string AddresseeName { get; set; }

        public int EpepUserId { get; set; }
    }

}
